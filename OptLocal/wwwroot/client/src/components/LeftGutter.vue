<template>
    <div id="gutter-container" :style="{visibility:visible ? 'visible' : 'hidden' }" v-html="gutter">
    </div>
</template>

<script>
    export default {
        name: 'LeftGutter',
        data() {
            return {
                gutter: '',
                visible: false
            }
        },
        mounted() {
            this.gutter = ExecutionVisualizer.createGutter('leftCodeGutterSVG');
        },
        methods: {
            show: function () {
                this.visible = true;
            },
            hide: function () {
                this.visible = false;
            }
        }
    }
</script>

<style>

</style>