const colors = [Color.FromArgb(255, 202, 10), Color.FromArgb(255, 102, 31), Color.FromArgb(255, 31, 53), Color.FromArgb(103, 0, 199), Color.FromArgb(15, 227, 255), Color.FromArgb(255, 0, 0), Color.FromArgb(0, 255, 0), Color.FromArgb(0, 0, 255)]

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
