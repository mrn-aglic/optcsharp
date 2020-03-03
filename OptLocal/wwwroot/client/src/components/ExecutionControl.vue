<template>
    <div class="col-12">
        <button id="bk" value="" class="btn btn-primary"
                :disabled="currentLine === 0" v-on:click="back()">
            <i class="fas fa-angle-left fa-lg"></i>
        </button>
        <button id="fd" class="btn btn-primary"
                :disabled="!hasForward()" v-on:click="forward()">
            <i class="fas fa-angle-right fa-lg"></i>
        </button>
    </div>
</template>

<script>

    export default {
        name: 'ExecutionControl',
        data() {
            return {
                currentLine: 0
            }
        },
        methods: {
            getData: function () {
                let self = this;
                return {
                    currentLine: self.currentLine
                };
            },
            notifyLineChange: function (eventName) {
                this.$emit(eventName, this.getData());
            },
            forward() {
                this.currentLine = this.currentLine + 1;
                this.notifyLineChange('forward');
            },
            back() {
                this.currentLine = this.currentLine - 1;
                this.notifyLineChange('backward');
            },
            hasForward() {
                return true;
            }
        }
    }

</script>

<style scoped>

</style>