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
                visualizer: null
            }
        },
        components: {},
        mounted() {
            // this.visualizer = this.crateVisInstance();
        },
        methods: {
            crateVisInstance: function () {
                return new ExecutionVisualizer(this.rootId, this.trace, this.options);
            },
            visualize: function (trace) {
                this.trace = trace;
                this.visualizer = this.crateVisInstance();
            },
            stepForward: function () {
                let vis = this.visualizer;

                vis.stepForward();
            },
            stepBack: function () {
                let vis = this.visualizer;

                vis.stepBack();
            }
        }
    }
</script>

<style scoped>

</style>