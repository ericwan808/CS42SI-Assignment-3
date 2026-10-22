using UnityEngine;

/// <summary>
/// Draws a leather drawstring dice bag into a texture at any size, so the
/// bag can be redrawn to fit however many dice are shown. The bottom
/// "body" part holds the dice; above it the leather is cinched by a cord
/// and ruffles out at the top.
/// </summary>
public static class LeatherBagArt
{
    private static readonly Color Leather = C(150, 96, 50);
    private static readonly Color LeatherDark = C(78, 45, 20);
    private static readonly Color Highlight = C(190, 130, 75);
    private static readonly Color Outline = C(45, 25, 10);
    private static readonly Color Stitch = C(225, 195, 140);
    private static readonly Color Cord = C(205, 170, 105);
    private static readonly Color CordDark = C(120, 90, 50);

    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="bodyHeight">Height of the part that holds the dice, from the bottom, in pixels.</param>
    /// <param name="detailScale">Size of fine details (seam, outline, grain); 1 suits ~150 pixels per die.</param>
    /// <param name="tightCorners">Less rounded bottom corners and a seam closer to the edge, for bags
    /// tall enough that the normal corners would curve into the bottom row of dice.</param>
    public static Texture2D Draw(int width, int height, int bodyHeight, float detailScale, bool tightCorners = false)
    {
        var shape = new BagShape(width, height, bodyHeight, tightCorners);
        int W = width, H = height;
        float k = Mathf.Max(0.5f, detailScale);

        // Coverage with 2x2 subsamples, for smooth edges.
        var coverage = new float[W * H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int hits = 0;
                if (shape.Inside(x + 0.25f, y + 0.25f)) hits++;
                if (shape.Inside(x + 0.75f, y + 0.25f)) hits++;
                if (shape.Inside(x + 0.25f, y + 0.75f)) hits++;
                if (shape.Inside(x + 0.75f, y + 0.75f)) hits++;
                coverage[y * W + x] = hits / 4f;
            }

        float[] dist = DistanceToEdge(coverage, W, H);

        float shadeDepth = W * 0.16f;
        // Tight bags keep the seam a fixed distance (relative to die size) from the edge.
        float stitchInset = tightCorners ? 7f * k : W * 0.045f;
        float cordThickness = Mathf.Max(2f * k, shape.TopHeight * 0.07f);
        float dashLength = 9f * k;
        var pixels = new Color32[W * H];

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int i = y * W + x;
                float alpha = coverage[i];
                bool onCord = Mathf.Abs(y + 0.5f - shape.CinchY) < cordThickness
                              && Mathf.Abs(x + 0.5f - shape.CenterX) < shape.NeckHalfWidth + cordThickness * 1.2f;
                if (alpha <= 0f && !onCord) continue;

                float d = dist[i];

                // Darker toward the edges, plus a soft highlight on the upper left.
                Color col = Color.Lerp(LeatherDark, Leather, Mathf.Pow(Mathf.Min(1f, d / shadeDepth), 0.6f));
                float hx = (x - (shape.CenterX - W * 0.15f)) / (W * 0.45f);
                float hy = (y - bodyHeight * 0.65f) / (bodyHeight * 0.6f + 1f);
                col = Color.Lerp(col, Highlight, Mathf.Max(0f, 1f - (hx * hx + hy * hy)) * 0.35f);

                // Leather grain.
                float grain = ValueNoise(x * 0.25f / k, y * 0.25f / k) * 0.6f + ValueNoise(x * 0.08f / k, y * 0.08f / k) * 0.4f;
                col *= 0.9f + 0.2f * grain;

                // Folds in the gathered leather above the body.
                if (y > bodyHeight)
                {
                    float fx = (x - shape.CenterX) / Mathf.Max(1f, shape.HalfWidth(y));
                    float fold = 0.5f + 0.5f * Mathf.Cos(fx * Mathf.PI * 3.5f);
                    float strength = y >= shape.CinchY ? 0.3f : 0.3f * Smooth((y - bodyHeight) / (shape.CinchY - bodyHeight));
                    col *= 1f - strength * fold;
                }

                // Dashed seam around the bottom and sides of the body.
                if (y < bodyHeight * 0.85f && Mathf.Abs(d - stitchInset) < 1.3f * k && d > 2f * k)
                {
                    if (Mathf.Repeat(shape.SeamPosition(x + 0.5f, y, stitchInset) / dashLength, 1f) < 0.55f)
                        col = Stitch;
                }

                // Dark outline.
                if (d < 2.2f * k)
                    col = Color.Lerp(Outline, col, Mathf.Max(0f, (d - k) / (1.2f * k)));

                if (onCord)
                {
                    col = CordColor(x, y - shape.CinchY, Mathf.Abs(y + 0.5f - shape.CinchY) / cordThickness, k);
                    alpha = 1f;
                }

                col.a = alpha;
                pixels[i] = col;
            }

        DrawCordTails(pixels, W, H, shape, cordThickness, k);

        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        tex.SetPixels32(pixels);
        tex.Apply();
        return tex;
    }

    // The two loose ends of the drawstring hanging over the shoulders, with knots.
    private static void DrawCordTails(Color32[] pixels, int W, int H, BagShape shape, float thickness, float k)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            float x0 = shape.CenterX + side * shape.NeckHalfWidth * 0.6f, y0 = shape.CinchY;
            float x1 = shape.CenterX + side * shape.NeckHalfWidth * 1.5f;
            float y1 = shape.BodyHeight + (shape.CinchY - shape.BodyHeight) * 0.1f;
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1)) * 2f));

            for (int s = 0; s <= steps; s++)
            {
                float t = s / (float)steps;
                float px = Mathf.Lerp(x0, x1, t) + side * Mathf.Sin(t * Mathf.PI) * shape.NeckHalfWidth * 0.25f;
                float py = Mathf.Lerp(y0, y1, t);
                float radius = t < 0.9f ? thickness * 0.5f : thickness * 0.95f;

                for (int yy = (int)(py - radius - 1); yy <= (int)(py + radius + 1); yy++)
                    for (int xx = (int)(px - radius - 1); xx <= (int)(px + radius + 1); xx++)
                    {
                        if (xx < 0 || xx >= W || yy < 0 || yy >= H) continue;
                        float dd = Vector2.Distance(new Vector2(xx + 0.5f, yy + 0.5f), new Vector2(px, py));
                        if (dd >= radius) continue;
                        Color col = CordColor(xx, yy, dd / radius, k);
                        col.a = 1f;
                        pixels[yy * W + xx] = col;
                    }
            }
        }
    }

    // Twisted rope look; edge is 0 at the cord's center and 1 at its edge.
    private static Color CordColor(float x, float y, float edge, float k)
    {
        float twist = 0.5f + 0.5f * Mathf.Sin((x + y * 2.2f) * 0.9f / k);
        Color col = Color.Lerp(CordDark, Cord, 0.45f + 0.55f * twist);
        return Color.Lerp(col, CordDark, Smooth((edge - 0.6f) / 0.4f));
    }

    // Approximate distance from each inside pixel to the outside (two-pass chamfer).
    private static float[] DistanceToEdge(float[] coverage, int W, int H)
    {
        const float Diagonal = 1.414f;
        var d = new float[W * H];
        for (int i = 0; i < d.Length; i++)
            d[i] = coverage[i] <= 0f ? 0f : float.MaxValue;

        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int i = y * W + x;
                if (d[i] == 0f) continue;
                float v = d[i];
                v = Mathf.Min(v, x > 0 ? d[i - 1] + 1f : 1f);
                if (y > 0)
                {
                    v = Mathf.Min(v, d[i - W] + 1f);
                    if (x > 0) v = Mathf.Min(v, d[i - W - 1] + Diagonal);
                    if (x < W - 1) v = Mathf.Min(v, d[i - W + 1] + Diagonal);
                }
                else v = Mathf.Min(v, 1f);
                d[i] = v;
            }

        for (int y = H - 1; y >= 0; y--)
            for (int x = W - 1; x >= 0; x--)
            {
                int i = y * W + x;
                if (d[i] == 0f) continue;
                float v = d[i];
                v = Mathf.Min(v, x < W - 1 ? d[i + 1] + 1f : 1f);
                if (y < H - 1)
                {
                    v = Mathf.Min(v, d[i + W] + 1f);
                    if (x < W - 1) v = Mathf.Min(v, d[i + W + 1] + Diagonal);
                    if (x > 0) v = Mathf.Min(v, d[i + W - 1] + Diagonal);
                }
                else v = Mathf.Min(v, 1f);
                d[i] = v;
            }
        return d;
    }

    /// <summary>The bag's outline, in texture pixels (y = 0 is the bottom).</summary>
    private readonly struct BagShape
    {
        public readonly float CenterX, BodyHeight, TopHeight, CinchY, NeckHalfWidth;
        private readonly float height, halfWidth, cornerRadius, flareHalfWidth, ruffleAmount;

        public BagShape(int width, int height, int bodyHeight, bool tightCorners)
        {
            this.height = height;
            CenterX = width / 2f;
            BodyHeight = bodyHeight;
            TopHeight = height - bodyHeight;
            halfWidth = width * 0.49f;
            cornerRadius = tightCorners
                ? Mathf.Min(width * 0.3f, bodyHeight * 0.35f)
                : Mathf.Min(width * 0.38f, bodyHeight * 0.5f);
            CinchY = bodyHeight + TopHeight * 0.45f;
            NeckHalfWidth = TopHeight * 0.38f;
            flareHalfWidth = Mathf.Min(TopHeight * 0.75f, halfWidth * 0.8f);
            ruffleAmount = TopHeight * 0.09f;
        }

        public float HalfWidth(float y)
        {
            if (y < 0f) return -1f;
            if (y < cornerRadius)
            {
                float dy = cornerRadius - y;
                return halfWidth - cornerRadius + Mathf.Sqrt(Mathf.Max(0f, cornerRadius * cornerRadius - dy * dy));
            }
            if (y < BodyHeight)
            {
                // Slight taper toward the top, like a filled pouch.
                float t = (y - cornerRadius) / Mathf.Max(1f, BodyHeight - cornerRadius);
                return halfWidth * (1f - 0.05f * t * t);
            }
            float shoulder = halfWidth * 0.95f;
            if (y < CinchY)
            {
                // Rounded shoulder: a quarter ellipse from the body into the neck.
                float t = (y - BodyHeight) / (CinchY - BodyHeight);
                return NeckHalfWidth + (shoulder - NeckHalfWidth) * Mathf.Sqrt(Mathf.Max(0f, 1f - t * t));
            }
            float u = (y - CinchY) / (height - CinchY);
            return Mathf.Lerp(NeckHalfWidth, flareHalfWidth, 1f - (1f - u) * (1f - u));
        }

        private float TopEdge(float x)
        {
            float wave = Mathf.Sin((x - CenterX) / flareHalfWidth * Mathf.PI * 3.5f + Mathf.PI / 2f);
            return height - 2f - ruffleAmount * (1f + wave);
        }

        public bool Inside(float x, float y)
        {
            return Mathf.Abs(x - CenterX) <= HalfWidth(y) && y <= TopEdge(x);
        }

        // Distance along the seam (bottom, then corner, then side), so stitches are evenly spaced.
        public float SeamPosition(float x, float y, float inset)
        {
            float dx = Mathf.Abs(x - CenterX);
            float straight = halfWidth - cornerRadius;
            if (y < cornerRadius && dx > straight)
                return straight + Mathf.Atan2(cornerRadius - y, dx - straight) * (cornerRadius - inset);
            if (y < cornerRadius)
                return dx;
            return straight + Mathf.PI / 2f * (cornerRadius - inset) + (y - cornerRadius);
        }
    }

    private static float Smooth(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    private static float Hash(int x, int y)
    {
        unchecked
        {
            uint h = (uint)x * 374761393u + (uint)y * 668265263u;
            h = (h ^ (h >> 13)) * 1274126177u;
            return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
        }
    }

    private static float ValueNoise(float x, float y)
    {
        int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
        float u = Smooth(x - xi), v = Smooth(y - yi);
        float a = Mathf.Lerp(Hash(xi, yi), Hash(xi + 1, yi), u);
        float b = Mathf.Lerp(Hash(xi, yi + 1), Hash(xi + 1, yi + 1), u);
        return Mathf.Lerp(a, b, v);
    }

    private static Color C(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f, 1f);
}
