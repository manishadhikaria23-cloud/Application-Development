// Minimal rich editor helper using contentEditable + execCommand
window.richEditor = {
    exec: function (command, value) {
        try {
            var editor = document.getElementById('editor');
            if (!editor) return;
            editor.focus();
            if (command === 'createLink') {
                var url = prompt('Enter URL', 'https://');
                if (!url) return;
                document.execCommand('createLink', false, url);
                return;
            }
            document.execCommand(command, false, value || null);
        } catch (e) {
            console.error('richEditor.exec error', e);
        }
    },

    getHtml: function () {
        var editor = document.getElementById('editor');
        return editor ? editor.innerHTML : '';
    },

    setHtml: function (html) {
        var editor = document.getElementById('editor');
        if (editor) editor.innerHTML = html || '';
    },

    getText: function () {
        var editor = document.getElementById('editor');
        return editor ? editor.innerText || '' : '';
    }
};