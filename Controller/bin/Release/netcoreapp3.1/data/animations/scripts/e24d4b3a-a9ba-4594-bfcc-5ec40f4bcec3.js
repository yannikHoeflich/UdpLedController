const colors = GetArg("colors");

const color = colors[Random(0, colors.length)];
const brightness = 0;
const change = 1;

const r;
const g;
const b;
const tempColor;

r = color.R / 100 * brightness;
g = color.G / 100 * brightness;
b = color.B / 100 * brightness;
tempColor = Color.FromArgb(r, g, b);

for (var i = 0; i < LedLength; i++) {
    SetColor(i, tempColor);
}

brightness += change;

if (brightness >= 100) {
    change *= -1;
}

if (brightness <= 0) {
    color = colors[Random(0, colors.length)];
    change *= -1;
}
