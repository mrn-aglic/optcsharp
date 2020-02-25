<template>
    <div id='app' class='container-fluid min-vh-100'>
        <div class='row justify-content-end'>
            <div class='col-10 col-md-10 col-sm-10 col-lg-10 col-xl-10'>
            </div>
        </div>
        <div class='row no-gutters h-100'>
            <workspace></workspace>
        </div>
    </div>
</template>

<script>
    
    import Workspace from "./components/Workspace.vue";
    
    export default {
        name: 'App',
        components: {
            Workspace
        }
    }
</script>

<style scoped>
    
</style>