<template>
    <div id="monaco-container">
    </div>
</template>

<script>

    import * as monaco from 'monaco-editor/esm/vs/editor/editor.api';

    const replaceValue = function (value) {

        const tab = RegExp('\\t', 'g');
        return value.replace(tab, '    ');
    };

    const createMonacoModel = function (code) {

        const model = monaco.editor.createModel(code, 'csharp');
        model.updateOptions({
            insertSpaces: true
        });
        return model;
    };

    export default {
        name: 'CodeEditor',
        props: ['editorData'],
        data() {
            return {
                editor: null,
                currentDecorations: [],
                currentLine: 0
            }
        },
        mounted() {
            const editor = monaco.editor.create(document.getElementById('monaco-container'), {
                value: replaceValue(''),
                theme: 'vs-dark',
                language: 'csharp',
                scrollBeyondLastLine: false,
                autoIndent: true,
                automaticLayout: true
            });

            const model = editor.getModel();
            model.updateOptions({
                insertSpaces: true
            });

            // let timeout = null;
            // editor.getModel().onDidChangeContent(event => {
            //
            //     this.$emit('editorContentChange', new LineCount(0, editor.getModel().getLineCount()));
            //     clearTimeout(timeout);
            //     timeout = setTimeout(() => {
            //
            //         console.log(editor.getValue());
            //     }, 2000);
            // });

            window.editor = editor;
            this.editor = editor;
        },
        methods: {
            setValue: function (code) {
                this.editor.setModel(createMonacoModel(code));
            },
            getValue: function () {
                return this.editor.getValue();
            },
            decorateLines: function (highlightData) {
                let self = this;
                let editor = self.editor;
                let model = editor.getModel();
                let decorations = [];
                const options = this.getLineDecorationOptions();
                for (let i = highlightData.startLine; i <= highlightData.endLine; i++) {
                    const range = new monaco.Range(
                        i,
                        model.getLineFirstNonWhitespaceColumn(i),
                        i,
                        model.getLineLastNonWhitespaceColumn(i)
                    );
                    decorations.push({range: range, options: options});
                }
                this.currentDecorations = editor.deltaDecorations(this.currentDecorations, decorations);
            },
            getLineDecorationOptions: function () {
                return {
                    isWholeLine: false,
                    inlineClassName: 'currentLine',
                    scrollBeyondLastLine: false
                }
            },
            setDecorations: function (range) {
                const options = this.getLineDecorationOptions();
                const decorations = [{
                    range: range,
                    options: options
                }];
                this.currentDecorations = this.editor.deltaDecorations(this.currentDecorations, decorations);
                // editor.getModel().matchBracket(new monaco.Position(5, editor.getModel().getLineContent(5).length))
            },
            getLine: function (lineNumber) {
                const model = editor.getModel();
                const currentLine = model.getLineContent(lineNumber);

            },
            getRangeForLine: function (line) {
                const model = editor.getModel();
                const columnStart = (line === 0) ? 0 : model.getLineFirstNonWhitespaceColumn(line);
                const columnStop = (line === 0) ? 0 : model.getLineLastNonWhitespaceColumn(line);
                return new monaco.Range(line, columnStart, line, columnStop);
            },
            setLine: function (number) {
                this.currentLine = number;
            },
            nextLine: function () {
                
                // this.currentLine += 1;
                // let range = this.getRangeForLine(this.currentLine);
                // this.setDecorations(range);
            },
            prevLine: function () {
                // this.currentLine -= 1;
                // let range = this.getRangeForLine(this.currentLine);
                // this.setDecorations(range);
            }
        }
    }
</script>

<style>
    #monaco-container {
        /*height: 100%;*/
    }

    .test {
        background-color: red;
    }

    .currentLine {
        background-color: #dcffe4;
    }
</style>