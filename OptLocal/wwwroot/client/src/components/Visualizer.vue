<template>
    <div :id="rootId"></div>
</template>

<script>

    export default {
        name: 'Visualizer',
        data() {
            return {
                rootId: 'opt-visualizer',
                trace: {
                    code: '',
                    trace: []
                },
                options: {
                    hideCode: true,
                    allowEditAnnotations: false
                },
                registerVisualizer: function (vis) {

                    return () => vis;
                },
                getVisualizer: () => {
                }
            }
        },
        components: {},
        mounted() {
            // this.visualizer = this.crateVisInstance();
        },
        methods: {
            crateVisInstance: function (codeEditor) {
                const monacoLineHeight = 49;
                console.log(codeEditor.getOption(monacoLineHeight))
                const editor = new CodeEditorInstance(codeEditor, e => e.getOption(monacoLineHeight));
                return new ExecutionVisualizer(this.rootId, this.trace, this.options, editor);
            },
            visualize: function (trace, gutter, codeEditor) {
                this.trace = trace;
                this.getVisualizer = this.registerVisualizer(this.crateVisInstance(codeEditor));
                gutter.show();
                let vis = this.getVisualizer();

                this.gutterSVG = gutter.$el.children[0];
                vis.highlightCodeLine(this.gutterSVG, true);
            },
            stepForward: function () {
                let vis = this.getVisualizer();

                vis.stepForward();
                vis.highlightCodeLine(this.gutterSVG, true);
            },
            stepBack: function () {
                let vis = this.getVisualizer();

                vis.stepBack();
                vis.highlightCodeLine(this.gutterSVG, true);
            }
        }
    }
</script>

<style scoped>

</style>