//h Init
const CloudColors = GetArg("cloudColors");
const StarColors = GetArg("starColors");
const BackgroundColor = GetArg("backgroundColor");

const Stars = [];
const Clouds = [];

//h add Opject
if (Random(0, 100) === 1) {
    var skip = false;
    for (var i = 0; i < Clouds.length; i++) {
        if (Clouds[i].index < 0) {
            skip = true;
            break;
        }
    }
    if (!skip) {
        var length = Random(20, 30)
        var newCloud = {
            index: -length,
            length: length,
            color: CloudColors[Random(0, CloudColors.length)]
        };
        Clouds.push(newCloud);
    }
}

if (Stars.length < 30) {
    if (Random(0, 10) === 1) {
        var newStar = {
            index: Random(0, LedLength),
            lifeTime: Random(10, 70),
            color: StarColors[Random(0, StarColors.length)]
        }
        Stars.push(newStar);
    }
}

//h Update

for (var i = 0; i < Stars.length; i++) {
    Stars[i].lifeTime--;
    if (Stars[i].lifeTime <= 0) {
        Stars.splice(i, 1)
        i--;
    }
}

for (var i = 0; i < Clouds.length; i++) {
    Clouds[i].index++;
    if (Clouds[i].index >= LedLength) {
        Clouds.splice(i, 1);
        i--;
    }
}

//H Render

for (var i = 0; i < LedLength; i++) {
    SetColor(i, BackgroundColor);
}

for (var i = 0; i < Clouds.length; i++) {
    for (var j = 0; j < Clouds[i].length; j++) {
        var index = j + Clouds[i].index;
        if (index < 0)
            continue;
        if (j < 10) {
            r = Clouds[i].color.R / 10 * j;
            g = Clouds[i].color.G / 10 * j;
            b = Clouds[i].color.B / 10 * j;

            SetColor(index, Color.FromArgb(r, g, b));
        } else if (j > Clouds[i].length - 10) {
            r = Clouds[i].color.R / 10 * ((j - Clouds[i].length) * -1);
            g = Clouds[i].color.G / 10 * ((j - Clouds[i].length) * -1);
            b = Clouds[i].color.B / 10 * ((j - Clouds[i].length) * -1);

            SetColor(index, Color.FromArgb(r, g, b));
        } else {
            SetColor(index, Clouds[i].color);
        }
    }
}

for (var i = 0; i < Stars.length; i++) {
    if (Stars[i].lifeTime >= 10) {
        SetColor(Stars[i].index, Stars[i].color);
    } else {
        r = Stars[i].color.R / 10 * Stars[i].lifeTime;
        g = Stars[i].color.G / 10 * Stars[i].lifeTime;
        b = Stars[i].color.B / 10 * Stars[i].lifeTime;
        SetColor(Stars[i].index, Color.FromArgb(r, g, b));
    }
}
