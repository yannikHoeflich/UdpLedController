﻿namespace Controller {
    class WebsiteConstants {
        public const string Html = @"
<!DOCTYPE html>

<html>

<head>
    <meta charset=""utf-8"" />
    <title>Led Controller</title>
    <link rel="" stylesheet"" href=""https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css""
          integrity=""sha384-JcKb8q3iqJ61gNV9KGb8thSsNjpSL0n8PARn9HuZOnIxN0hoP+VmmDGMN5t9UJ0Z"" crossorigin=""anonymous"">
</head>

<body>
    <header>
    </header>
    <main>
        <button type=""button"" class=""btn btn-primary"" onclick=""ScanDevices()"">scan for devices</button>
        <button type=""button"" class=""btn btn-primary"" onclick=""ReloadDevices()"">reload Animations</button>

        <label for=""dimFactor"" class=""form-label"">Dimming</label>
        <input type=""range"" class=""form-range"" min=""0"" max=""1"" step=""0.01"" id=""dimFactor"" oninput=""setDimFactor(this)"">

        <ul class=""list-group"" id=""AnimationList"">
        </ul>
    </main>


    <script>
        loadAnimations();
        async function loadAnimations() {
            let response = await fetch(""/api?method=getAnimations"");

            if (response.ok) { // if HTTP-status is 200-299
                // get the response body (the method explained below)
                let animations = await response.json();
                console.log(animations);

                var currentAnimation = await CurrentAnimation();

                var ListElement = document.getElementById(""AnimationList"");
                ListElement.innerHTML = """";
                var i = 0;
                animations.forEach(element => {
                    if (currentAnimation)
                        ListElement.innerHTML += ""<a "" + (element.name == currentAnimation.name ? """" : ""href=\""#\"""") + ""\"" onclick=\""SetAnimation("" + i + "")\""><li class=\""list-group-item\""> "" + element.name + ""</li></a>"";
                    else
                        ListElement.innerHTML += "" <a href=\""#\"" onclick=\""SetAnimation("" + i + "")\""><li class=\""list-group-item\"">"" + element.name + ""</li></a>"";
                    i++;
                });
            } else {
                alert(""HTTP-Error: "" + response.status);
            }
        }

        function animationsEquals(animation1, animation2) {
            if (animation1 == null || animation2 == null)
                return animation1 == animation2;
            return animation1.name == animation2.name;
        }

        function arrayEquals(a, b) {
            return Array.isArray(a) &&
                Array.isArray(b) &&
                a.length === b.length &&
                a.every((val, index) => val === b[index]);
        }

        async function CurrentAnimation() {
            let response = await fetch(""/api?method=getCurrentAnimation"");

            if (response.ok) { // if HTTP-status is 200-299
                // get the response body (the method explained below)
                let json = await response.json();
                return json;
                console.log(json);

            } else {
                alert(""HTTP-Error: "" + response.status);
            }
        }

        async function ScanDevices() {
            let response = await fetch(""/api?method=scanDevices"");

            if (response.ok) { // if HTTP-status is 200-299
                // get the response body (the method explained below)
                let json = await response.json();
                console.log(json);
            } else {
                alert(""HTTP-Error: "" + response.status);
            }
        }


        async function ReloadDevices() {
            let response = await fetch(""/api?method=scanAnimations"");

            if (response.ok) { // if HTTP-status is 200-299
                // get the response body (the method explained below)
                let json = await response.json();
                console.log(json);
            } else {
                alert(""HTTP-Error: "" + response.status);
            }
            await loadAnimations();
        }

        async function SetAnimation(id) {
            let response = await fetch(""/api?method=setAnimation&animation="" + id);

            if (response.ok) { // if HTTP-status is 200-299
                // get the response body (the method explained below)
                let json = await response.json();
                console.log(json);
            } else {
                alert(""HTTP-Error: "" + response.status);
            }
            loadAnimations();
        }

        async function setDimFactor(sender) {
            let response = await fetch(""/api?method=setDimmng&factor="" + sender.value);

            if (response.ok) { // if HTTP-status is 200-299
                // get the response body (the method explained below)
                let json = await response.json();
                console.log(json);
            } else {
                alert(""HTTP-Error: "" + response.status);
            }
        }

        async function getDimFactor(sender) {
            let response = await fetch(""/api?method=getDimmng"");

            if (response.ok) { // if HTTP-status is 200-299
                // get the response body (the method explained below)
                let json = await response.json();
                document.getElementById(""dimFactor"").value = json;
            } else {
                alert(""HTTP-Error: "" + response.status);
            }
        }
        getDimFactor();
    </script>
    <script src=""https://code.jquery.com/jquery-3.5.1.slim.min.js""
            integrity="" sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj"" crossorigin=""anonymous""></script>
    <script src=""https://cdn.jsdelivr.net/npm/popper.js@1.16.1/dist/umd/popper.min.js""
            integrity="" sha384-9/reFTGAW83EW2RDu2S0VKaIzap3H66lZH81PoYlFhbGU+6BZp6G7niu735Sk7lN"" crossorigin=""anonymous""></script>
    <script src=""https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js""
            integrity="" sha384-B4gt1jrGC7Jh4AgTPSdUtOBvfO8shuf57BaghqFfPlYxofvL8/KUEfYiJOMMV+rV"" crossorigin=""anonymous""></script>

</body>

</html>
";
    }
}
