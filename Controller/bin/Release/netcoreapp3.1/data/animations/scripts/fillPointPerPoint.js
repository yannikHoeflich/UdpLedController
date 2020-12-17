const colors = [Color.FromArgb(255, 202, 10), Color.FromArgb(255, 102, 31), Color.FromArgb(255, 31, 53), Color.FromArgb(103, 0, 199), Color.FromArgb(15, 227, 255), Color.FromArgb(255, 0, 0), Color.FromArgb(0, 255, 0), Color.FromArgb(0, 0, 255)]

const oldColor = colors[Random(0, colors.length)];
const newColor = colors[Random(0, colors.length)];
const index = 0;
const Fullniss = 1;

function GenerateNewColor() {
    oldColor = newColor;
    while (oldColor == newColor)
        newColor = colors[Random(0, colors.length)];
}

for (var i = 0; i < LedLength; i++) {
    if (i == index)
        SetColor(i, newColor);
    else {
        if (i > LedLength - Fullniss) {
            SetColor(i, newColor);
        } else {
            SetColor(i, oldColor);
        }
    }
    if (index >= LedLength - Fullniss - 1) {
        index = 0;
        Fullniss++;
        if (Fullniss == LedLength) {
            oldColor = newColor;
            newColor = colors[Random(0, colors.length)];
            Fullniss = 1;
        }
    }
}
index += 2;
