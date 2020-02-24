import Vue from 'vue';
import VueRouter from 'vue-router';

import App from './App.vue';

Vue.config.productionTip = false;

// Ensure we checked auth before each page load.
// router.beforeEach((to, from, next) =>
//     Promise.all([store.dispatch(CHECK_AUTH)]).then(next)
// );

export const diagramEventBus = new Vue();

const routes = [
    {path: '/', name: 'home'},
];

const router = new VueRouter({mode: 'history', routes});

new Vue({
    router,
    render: h => h(App)
}).$mount('#app');
