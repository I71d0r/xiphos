//--Notable--
// Custom JS runtime methods has to be registered within jQuery validation framework
// so the unobtrusive validation can match the form elements attributes.
$.validator.addMethod("notes-only", function (value, element, params) {
    var notesStrings = ["C", "C#", "DB", "D", "D#", "EB", "E", "F", "F#", "GB", "G", "G#", "AB", "A", "A#", "BB", "B"];

    var notes = value.toUpperCase().split(" ");

    for (var i = 0; i < notes.length; i++) {
        var n = notes[i].trim(" ");

        var ok = false;

        for (var j = 0; j < notesStrings.length; j++) {
            if (n === notesStrings[j]) {
                ok = true;
                break;
            }
        }

        if (!ok) return false;
    }

    return true;
});

$.validator.unobtrusive.adapters.addBool("notes-only");