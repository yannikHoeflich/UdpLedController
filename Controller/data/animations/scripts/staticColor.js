var color = Color.FromArgb(GetArg("r"), GetArg("g"), GetArg("b"))

for (var i = 0; i < LedLength; i++) {
    SetColor(i, color);
}
