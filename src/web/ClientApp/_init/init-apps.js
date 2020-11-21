// import './css/site.css'
import 'core-js/features/promise'
import 'core-js/features/array'
import Vue from "vue";

// example import
import VuePage from '../Components/VuePage.vue'

// todo revist this fancy stuff we can do in the config
// import axios from 'axios'
// import router from './router/index'
// import store from './store'
// import { sync } from 'vuex-router-sync'

// import { FontAwesomeIcon } from './icons'

// // Registration of global components
// Vue.component('icon', FontAwesomeIcon)

// Vue.prototype.$http = axios

// sync(store, router)

const init = (selector, component) => {
    // the theory here is that we will try to initialize all of our 'apps' when this script is ran
    // yet we will only load the apps for where the current page has the target selector
    // so, check for the selector and if present - create the Vue instance and mount.
    let elem = document.querySelector(selector);
    if (elem) {
        const app = new Vue({
            // store, router,
            ...component
        });

        app.$mount(elem);
    } else {
        // console.log(`NOT LOADED selector=${selector}`);
    }
};

init('#appVuePage', VuePage);

