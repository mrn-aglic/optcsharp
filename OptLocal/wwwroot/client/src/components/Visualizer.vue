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
            crateVisInstance: function () {
                return new ExecutionVisualizer(this.rootId, this.trace, this.options, {});
            },
            visualize: function (trace, gutter) {
                this.trace = trace;
                this.getVisualizer = this.registerVisualizer(this.crateVisInstance());
                gutter.show();
                let vis = this.getVisualizer();

                this.gutterSVG = gutter.$el.children[0];
                vis.highlightCodeLine(this.gutterSVG, null, null, true);
            },
            stepForward: function () {
                let vis = this.getVisualizer();

                vis.stepForward();
                vis.highlightCodeLine(this.gutterSVG, null, null, true);
            },
            stepBack: function () {
                let vis = this.getVisualizer();

                vis.stepBack();
            }
        }
    }
</script>

<style scoped>

</style>