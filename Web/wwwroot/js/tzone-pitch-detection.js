var audioContext = null;
var canvas = null;
var fftCanvas = null;

var fftSize = 32768; // FFT buckets
var maxDisplayFrequency = 4092; // Cutoff frequency in Hz
var binCount = 0;   // Number of FFT bins up to cutoff frequency - calculated later from the sample rate

// Notes and frequencies - Pythagorean scale
var notes = [           // c0     c1     c2      c3      c4      c5       c6       c7       c8
    { n: ["C"],        f: [16.35, 32.70, 65.41,  130.81, 261.63, 523.25, 1046.50, 2093.00, 4186.01] },
    { n: ["C#", "Db"], f: [17.32, 34.65, 69.30,  138.59, 277.18, 554.37, 1108.73, 2217.46, 4434.92] },
    { n: ["D"],        f: [18.35, 36.71, 73.42,  146.83, 293.66, 587.33, 1174.66, 2349.32, 4698.63] },
    { n: ["D#", "Eb"], f: [19.45, 38.89, 77.78,  155.56, 311.13, 622.25, 1244.51, 2489.02, 4978.03] },
    { n: ["E"],        f: [20.60, 41.20, 82.41,  164.81, 329.63, 659.25, 1318.51, 2637.02, 5274.04] },
    { n: ["F"],        f: [21.83, 43.65, 87.31,  174.61, 349.23, 698.46, 1396.91, 2793.83, 5587.65] },
    { n: ["F#", "Gb"], f: [23.12, 46.25, 92.50,  185.00, 369.99, 739.99, 1479.98, 2959.96, 5919.91] },
    { n: ["G"],        f: [24.50, 49.00, 98.00,  196.00, 392.00, 783.99, 1567.98, 3135.96, 6271.93] },
    { n: ["G#", "Ab"], f: [25.96, 51.91, 103.83, 207.65, 415.30, 830.61, 1661.22, 3322.44, 6644.88] },
    { n: ["A"],        f: [27.50, 55.00, 110.00, 220.00, 440.00, 880.00, 1760.00, 3520.00, 7040.00] },
    { n: ["A#", "Bb"], f: [29.14, 58.27, 116.54, 233.08, 466.16, 932.33, 1864.66, 3729.31, 7458.62] },
    { n: ["B"],        f: [30.87, 61.74, 123.47, 246.94, 493.88, 987.77, 1975.53, 3951.07, 7902.13] }
];


// Initializes the sound capture
function init() {
    canvas = $("#fftCanvas")[0];
    fftCanvas = canvas.getContext("2d");

    if (!navigator.getUserMedia)
        navigator.getUserMedia = navigator.webkitGetUserMedia || navigator.mozGetUserMedia;
    if (!navigator.cancelAnimationFrame)
        navigator.cancelAnimationFrame = navigator.webkitCancelAnimationFrame || navigator.mozCancelAnimationFrame;
    if (!navigator.requestAnimationFrame)
        navigator.requestAnimationFrame = navigator.webkitRequestAnimationFrame || navigator.mozRequestAnimationFrame;

    navigator.getUserMedia(
        {
            "audio": {
                "mandatory": {
                    "googEchoCancellation": "false",
                    "googAutoGainControl": "false",
                    "googNoiseSuppression": "false",
                    "googHighpassFilter": "false"
                },
                "optional": []
            },
        }, bindAudioStream, function (e) {
            alert("Error: Unable to get the audio stream.");
            console.log(e);
        });

}

function bindAudioStream(stream) {
    audioContext = new (window.AudioContext || window.webkitAudioContext)();

    var inputPoint = audioContext.createGain();

    var realAudioInput = audioContext.createMediaStreamSource(stream);
    var audioInput = realAudioInput;
    audioInput.connect(inputPoint);

    // the number of fft bins up to cutoff frequency
    binCount = Math.ceil(maxDisplayFrequency * fftSize / audioContext.sampleRate);

    var analyserNode = audioContext.createAnalyser();
    analyserNode.fftSize = fftSize;
    analyserNode.smoothingTimeConstant = 0.9;
    inputPoint.connect(analyserNode);
    updateAnalyzer(analyserNode);
}

// Data sampling event
function updateAnalyzer(analyzer) {
    var bufferLength = analyzer.frequencyBinCount;
    var data = new Uint8Array(bufferLength);
    analyzer.getByteFrequencyData(data);
    drawFrequencies(data);

    var noteIndex = findDominantNote(data);

    if (noteIndex == null) {
        $("#fLabel").text("-");
        submitNote(null);
    }
    else {
        if (notes[noteIndex].n.length > 1) {
            $("#fLabel").html(`${noteToHtml(notes[noteIndex].n[0])} / ${noteToHtml(notes[noteIndex].n[1])}`);
        }
        else {
            $("#fLabel").html(noteToHtml(notes[noteIndex].n[0]));
        }

        submitNote(notes[noteIndex].n);
    }

    setTimeout(function () {
        updateAnalyzer(analyzer);
    }, 20);
}

// Draws the spectrum
function drawFrequencies(data) {
    var grd = fftCanvas.createLinearGradient(0, 0, 0, canvas.height);
    grd.addColorStop(0, "#171717");
    grd.addColorStop(1, "#323232");

    fftCanvas.fillStyle = grd;
    fftCanvas.fillRect(0, 0, canvas.width, canvas.height);

    var barCount = 100;
    var barBins = 1.0 * binCount / barCount;
    var barWidth = 1.0 * canvas.width / barCount;
    var barHeightDivider = 256.0 / canvas.height;
    var sum = 0;
    var bar = 0;

    for (var i = 0; i < binCount + 1; i++) {
        sum += data[i];

        if (i > 0 && i >= barBins * (bar + 1)) {
            var barHeight = sum / barBins / barHeightDivider;

            fftCanvas.fillStyle = `rgb(${barHeight / 4}, ${barHeight / 2}, ${barHeight})`;
            
            fftCanvas.fillRect(bar * barWidth, canvas.height - barHeight, barWidth, barHeight);
            sum = 0;
            bar++;
        }
    }
}

// Very simple algorithm for finding the dominant note from the FFT spectrum
// Please note this algorithm is just for demonstration purposes and you
// can find far better and fine tuned algorithms on the internet.
// https://en.wikipedia.org/wiki/Pitch_detection_algorithm
function findDominantNote(data) {
    var lowSignalStrength = true;

    // Lets collect 100 peaks - this number is important so we capture the lowe spikes
    var x = 100;
    var topXamp = [];
    var topXfreq = [];
    var minAmpInTopX = 0;
    var noiseCutoff = 150;

    for (var i = 1; i < binCount - 1; i++) {
        var f = i * audioContext.sampleRate / fftSize;

        // The place where amplitude derivation changes from positive 
        // to negative is roughly the peak.
        var amp = data[i];
        var dp = data[i] - data[i - 1];
        var dn = data[i + 1] - data[i];

        if (data[i] > noiseCutoff && amp > minAmpInTopX && dp >= 0 && dn < 0) {
            topXamp.push(amp);
            topXfreq.push(f);

            if (topXamp.length > x) {
                topXamp.shift();
                topXfreq.shift();
            }

            for (var t = 0; t < topXamp.length; t++) {
                if (topXamp[t] < minAmpInTopX) minAmpInTopX = topXamp[t];
            }

            lowSignalStrength = false;
        }
    }

    if (lowSignalStrength) return null;

    // Assume the dominant frequency is the lowest peak that is high enough.
    // However, if has to be taken in to account when the lowest peak 
    // has a significantly lower amplitude.
    // Therefore the weighting function multiplies amplitude by log of the
    // frequency. That solution prefers lower frequencies but cares 
    // also about amplitude.
    var bestScore = 0;
    var dominantFrequency = 0;

    for (var p = 0; p < topXfreq.length; p++) {
        var score = topXamp[p] / (Math.log(topXfreq[p]));

        if (score > bestScore) {
            dominantFrequency = topXfreq[p];
            bestScore = score;
        }
    }

    // find the closest note
    var minDiff = 50000;
    var noteIndex = 0;
    var octaveIndex = 0;

    for (var n = 0; n < notes.length; n++) {
        for (var o = 0; o < notes[n].f.length; o++) {
            var diff = Math.abs(notes[n].f[o] - dominantFrequency);

            if (diff < minDiff) {
                minDiff = diff;
                noteIndex = n;
                octaveIndex = o;
            }
        }
    }

    // debug label update
    $("#debugData").html(`n=${notes[noteIndex].n},<br/> o=${octaveIndex},<br/> f=${Math.round(dominantFrequency)}Hz`);

    return noteIndex;
}

$().ready(init);