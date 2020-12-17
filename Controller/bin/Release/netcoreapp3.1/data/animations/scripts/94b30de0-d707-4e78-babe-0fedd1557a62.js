const colors = GetArg("colors");

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
