using System.Collections.Generic;
using System.Net;

namespace Controller {
    internal class OutputData {
        public byte[] buffer;
        public List<Cookie> NewCookies = new List<Cookie>( );
        public ContentType ContentType = ContentType.text_html;
        public string ContentTypeString => ContentType.ToString().Replace('_', '/');
    }

    enum ContentType {
        text_plain,
        audio_aac,
        image_bmp,
        text_css,
        text_csv,
        application_msword,
        image_gif,
        text_html,
        image_jpeg,
        text_javascript,
        application_json,
        audio_mpeg,
        font_otf,
        image_png,
        image_svg,
        application_zip,
        text_xml
    }
}
