<template>
    <b-form-select id="csharp-examples" v-model="selectedExample"
                   v-on:change="exampleChanged"
                   :options="examples"
                   value-field="code"
                   text-field="name"
                   class="mb-3"
    >
        <template v-slot:first>
            <b-form-select-option :value="null">-- Odaberi gotov primjer --</b-form-select-option>
        </template>
    </b-form-select>
</template>

<script>

    import {BFormSelect, BFormSelectOption} from 'bootstrap-vue';
    import Examples from '../examples/Examples.js';

    export default {
        name: 'CSharpExamples',
        components: {BFormSelect, BFormSelectOption},
        data() {
            return {
                selectedExample: null,
                examples: []
            }
        },
        mounted() {
            this.examples = Examples;
        },
        methods: {
            exampleChanged: function () {
                this.$emit('change', this.selectedExample);
            }
        }
    }
</script>

<style>

</style>