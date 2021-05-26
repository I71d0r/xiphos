function AnimatedLogo(element, text) {
    var index = 0;
    var phase = 0;
    var wave = "_,.-'``'-.,_,.-'``'-.,_,.-'``'-.,_,.-'``'-.,_,.-'``'-.,";

    var animate = function () {
        $("#" + element).text(wave.substring(index, index + 13) + "[ " + text + " ]" + wave.substring(index + 14, index + 25));

        index = index + 1 > 10 ? 0 : index + 1;

        if (phase < 45) {
            phase++;
            setTimeout(animate, 10 + phase);
        }
    }

    $("#" + element).click(function () {
        index = 0;
        phase = 0;
        animate();
    });

    animate();
};