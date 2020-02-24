<template>
    <div class="col-12">
        <div class="row col-6">
            <examples v-on:change="exampleChange"></examples>
        </div>
        <div class="row col-6">
            <button id="compile" class="btn btn-primary" v-on:click="compile()">
                Vizualiziraj izvršavanje
            </button>
        </div>
        <div class="row h-auto mt-2 mnh-90"> <!--h-75 ako ne zelimo cijelu visinu-->
            <div class="col-6">
                <b-card class="h-100">
                    <b-tabs class="h-100" lazy>
                        <b-tab v-for="editor in editors" :key="editor.tabKey" :title="editor.title">
                            <code-editor :ref="editor.tabKey" :editor-data="editor"></code-editor>
                        </b-tab>
                    </b-tabs>
                </b-card>
            </div>
            <div class="col-6">
                <visualizer :ref="visualizer"></visualizer>
            </div>
        </div>
        <div class="row mxh-10">
            <execution-control v-on:forward="forward()" v-on:backward="backward()"></execution-control>
        </div>
        <!--        <visualization-canvas></visualization-canvas>-->
    </div>
</template>

<script>

    import CodeEditor from './CodeEditor.vue';
    import Visualizer from './Visualizer.vue';
    import Examples from './Examples.vue';
    import ExecutionControl from './ExecutionControl.vue';
    import {BCard, BTabs, BTab} from 'bootstrap-vue';

    import Vue from 'vue';
    import {getCSharpTraceData} from '../Repositories/Compilation.js';

    export const workspaceEventBus = new Vue();

    export default {
        name: 'Workspace',
        components: {
            Visualizer,
            CodeEditor,
            ExecutionControl,
            BTabs,
            BTab,
            BCard,
            Examples
        },
        data() {
            return {
                compilationData: {},
                enteredMain: false,
                nextLineData: {},
                editors: [],
                visualizer: 'visualizer',
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
            getVisualizer: function(){
                return this.$refs[this.visualizer];
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
                    
                    self.visComponent.visualize(data);
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
            },
            backward: function () {
                let editor = this.getCurrentEditor();
                editor.prevLine();
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

    .mnh-90{
        min-height: 90%
    }
    .mxh-10{
        max-height: 10%
    }
    
</style>