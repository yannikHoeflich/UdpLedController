const color;
const baseColor = GetArg("color");

const AlarmHour = GetArg("hour");
const AlarmMinute = GetArg("minute");
const AlarmTimeBefore = GetArg("fade_in_minutes");

const StartAlarmHour = AlarmMinute - AlarmTimeBefore < 0 ? AlarmHour - 1 : AlarmHour;
const StartAlarmMinute = AlarmMinute - AlarmTimeBefore < 0 ? 60 + (AlarmMinute - AlarmTimeBefore) : AlarmMinute - AlarmTimeBefore;

if ((Time.Hour == StartAlarmHour && Time.Minute > StartAlarmMinute) || (Time.Hour == AlarmHour && Time.Minute < AlarmMinute && Time.Minute > (AlarmMinute - AlarmTimeBefore))) {
    var seconds;
    if (AlarmHour != StartAlarmHour) {
        if (Time.Hour == AlarmHour) {
            seconds = ((AlarmMinute - Time.Minute) * 60) + (60 - Time.Second);
        } else {
            seconds = ((60 - Time.Minute) * 60) + (60 - Time.Second);
            var tempAlarmHour = Time.Hour;
            while (tempAlarmHour < StartAlarmHour) {
                seconds += 3600;
            }
            seconds += AlarmMinute * 60;
        }
    } else {
        seconds = ((AlarmMinute - (Time.Minute - AlarmTimeBefore)) * 60) - (60 - Time.Second);
    }

    var brightness = (((AlarmTimeBefore * 60 - seconds) / (AlarmTimeBefore * 60)));
    var r = (brightness * baseColor.R);
    var g = (brightness * baseColor.G);
    var b = (brightness * baseColor.B);
    color = Color.FromArgb(r, g, b);
    Log(seconds + " " + color)
    for (var i = 0; i < LedLength; i++) {
        SetColor(i, color);
    }
}
