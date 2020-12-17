const color;

const AlarmHour = GetArg("hour");
const AlarmMinute = GetArg("minute");
const AlarmTimeBefore = 30;

if (Time.Hour == AlarmHour && Time.Minute > AlarmMinute - AlarmTimeBefore) {
    if (Time.Minute > AlarmMinute) {
        color = Color.FromArgb(255, 255, 255);
    } else {
        var seconds = (Time.Minute - 20) * 60 + Time.Second;
        var brightness = (seconds / (AlarmTimeBefore * 60)) * 255;
        brightness = brightness % 256;
        color = Color.FromArgb(brightness, brightness, brightness);
    }
} else {
    color = Color.FromArgb(0, 0, 0);
}
for (var i = 0; i < LedLength; i++) {
    SetColor(i, color);
}
