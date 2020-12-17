const hue = [255, 0, 0];
const from = 0;
const to = 1;

hue[from] -= 5;
hue[to] += 5;
if (hue[from] == 0) {
    from++;
    if (from >= 3)
        from = 0;
}
if (hue[to] == 255) {
    to++;
    if (to >= 3)
        to = 0;
}
for (var i = 0; i < LedLength; i++) {
    SetColor(i, Color.FromArgb(hue[0], hue[1], hue[2]));
}
