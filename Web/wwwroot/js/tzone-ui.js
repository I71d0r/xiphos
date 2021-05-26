var currentMelody = [];
var noteElements = [];
var melodyIndex = 0;
var score = 0;
var noteProgress = 0;
var running = false;

function noteToHtml(note) {
    var result = note;

    if (result.length > 2) result = "?"; // not a known note
    if (result.endsWith("b")) result = result.substr(0, result.length - 1) + "<sup>b</sup>";

    return result;
}

function loadMelodies(maxCount, filterString, preload) {
    $.getJSON("/Home/FetchMelodies", { limit: maxCount, filter: filterString },
        function (data) {
            var table = $("#melodyTable");
            table.empty();

            $.each(data.melodies, function (index, item) {
                table.append(`<tr class="itemRow"><td mid="${item.id}">${item.name}</td></tr>`);
            });

            $(".itemRow").click(function (event) {
                loadMelody(event.target.attributes.mid.value);
            });

            $("#moreDataLabel").html(data.filteredCount > 0 ? `And ${data.filteredCount} more...` : "");

            if (data.melodies.length > 0 && preload) {
                loadMelody(data.melodies[0].id); // load first on the list
            }
        }
    );
}

function loadMelody(mid) {
    $.getJSON("/Home/FetchMelody", { id: mid },
        function (melody) {

            var melodyDiv = $("#melody");
            melodyDiv.fadeOut("fast", function() {
                melodyDiv.empty();
                currentMelody = [];
                noteElements = [];

                $.each(melody.data.split(" "), function (index, item) {
                    currentMelody.push(item);

                    var elements = $(`<div class='bg-light text-dark rounded m-1 p-2 note'>${noteToHtml(item)}</div>`);
                    melodyDiv.append(elements);
                    noteElements.push(elements[0]);
                });

                resetMelody(false);

                melodyDiv.slideDown("slow");
            });
        }
    );
}

function resetMelody(start) {
    $("#winLabel").empty();

    for (var i = 0; i < noteElements.length; i++) {
        noteElements[i].classList.remove("bg-success");
        noteElements[i].classList.add("bg-light");
    }

    melodyIndex = 0;

    if (start) {
        score = currentMelody.length * 500;
        running = true;
    } else {
        score = 0;
    }
    updateProgress();
}

// Submit a detected note to the UI
function submitNote(notes) {
    if (melodyIndex > currentMelody.length - 1) return;

    var found = false;

    if (notes != null) {
        for (var i = 0; i < notes.length; i++) {
            if (currentMelody[melodyIndex] === notes[i]) {
                found = true;
                break;
            }
        }
    }

    if (found) {
        noteProgress += 6;
    } else {
        if (noteProgress > 0) noteProgress--;
    }

    if (noteProgress >= 100) {
        nextNote(true);
        noteProgress = 0;
    }

    if (score > 0) score--;

    updateProgress();
}

// Progress one note in the melody
function nextNote() {
    noteElements[melodyIndex].classList.remove("bg-light");
    noteElements[melodyIndex].classList.add("bg-success");

    melodyIndex++;

    var labelDiv = $("#winLabel");
    labelDiv.empty();

    if (melodyIndex >= currentMelody.length) {
        var la = labelDiv.html("<h1>You win!</h1>");
        la.fadeIn();
        running = false;
    } else {
        var lb = labelDiv.html("<h1>Nice!</h1>");
        lb.fadeIn();
        lb.fadeOut();
    }
}

function updateProgress() {
    $("#scoreLabel").html(`Score: ${score}`);
    $(".progress-bar").css({ "width": `${noteProgress}%` });
}