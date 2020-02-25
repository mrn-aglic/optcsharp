<template>
    <div class="col-12 h-100">
        <div class="row col-6">
            <examples v-on:change="exampleChange"></examples>
        </div>
        <div class="row col-6">
            <button id="compile" class="btn btn-primary" v-on:click="compile()">
                Vizualiziraj izvršavanje
            </button>
        </div>
        <div class="row mt-2 h-75 d-flex flex-column"> <!--h-75 ako ne zelimo cijelu visinu-->
            <div class="col-6">
                <b-card class="h-100">
                    <b-tabs class="h-100" lazy>
                        <b-tab v-for="editor in editors" :key="editor.tabKey" :title="editor.title">
                            <b-row class="h-75">
                                <left-gutter :ref="gutter" class="col-1 m-0 p-0 flex-grow-1"></left-gutter>
                                <code-editor :ref="editor.tabKey" :editor-data="editor" class="col-11 flex-grow-1"></code-editor>
                            </b-row>
                        </b-tab>
                    </b-tabs>
                </b-card>
            </div>
            <div class="col-6">
                <visualizer :ref="visualizer"></visualizer>
            </div>
        </div>
        <div class="row">
            <execution-control v-on:forward="forward()" v-on:backward="backward()"></execution-control>
        </div>
    </div>
</template>

<script>

    import CodeEditor from './CodeEditor.vue';
    import LeftGutter from './LeftGutter.vue';
    import Visualizer from './Visualizer.vue';
    import Examples from './Examples.vue';
    import ExecutionControl from './ExecutionControl.vue';
    import {BRow, BCard, BTabs, BTab} from 'bootstrap-vue';

    import Vue from 'vue';
    import {getCSharpTraceData} from '../Repositories/Compilation.js';

    export const workspaceEventBus = new Vue();

    export default {
        name: 'Workspace',
        components: {
            LeftGutter,
            Visualizer,
            CodeEditor,
            ExecutionControl,
            BTabs,
            BTab,
            BCard,
            BRow,
            Examples
        },
        data() {
            return {
                compilationData: {},
                enteredMain: false,
                nextLineData: {},
                editors: [],
                visualizer: 'visualizer',
                gutter: 'gutter',
                tabKey: 0,
                currentTab: -1
            }
        },
        mounted() {
            this.addTab();
            this.currentTab = 0;
            this.visComponent = this.getVisualizer();
        },
        methods: {
            getCurrentEditor: function () {
                return this.$refs[this.currentTab][0];
            },
            getVisualizer: function () {
                return this.$refs[this.visualizer];
            },
            getGutter: function () {
                return this.$refs[this.gutter][0];
            },
            compile: function () {
                let self = this;
                let editor = this.getCurrentEditor();
                let code = editor.getValue();

                let workspaceData = {
                    code: code,
                    rawInputJson: [] // TODO implement
                };

                getCSharpTraceData(workspaceData).then(data => {

                    let gutter = self.getGutter();
                    self.visComponent.visualize(data, gutter);
                });
                // getCSharpTraceData(workspaceData).then(data => {
                //     this.compilationData = data;
                //    
                //     console.log(this.compilationData);
                // });
            },
            forward: function () {
                let editor = this.getCurrentEditor();
                editor.nextLine();
                this.visComponent.stepForward();
            },
            backward: function () {
                let editor = this.getCurrentEditor();
                editor.prevLine();
                this.visComponent.stepBack();
            },
            addEditor: function () {
                let self = this;
                let title = self.editors.length === 0 ? 'Program.cs' : 'untitled.cs';
                self.editors.push({
                    tabKey: self.tabKey,
                    title: title
                });
                self.tabKey += 1;
            },
            addTab: function () {
                this.addEditor()
            },
            exampleChange: function (code) {
                let editor = this.getCurrentEditor();
                editor.setValue(code);
            }
        }
    }

</script>

<style scoped>

    .mnh-90 {
        min-height: 90%
    }

    .mxh-10 {
        max-height: 10%
    }

</style>