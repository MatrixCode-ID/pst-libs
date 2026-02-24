import { createRouter, createWebHashHistory } from "vue-router";
import HomePage from "./views/HomePage.vue";
import TocPage from "./views/TocPage.vue";
import HelpOverviewPage from "./views/HelpOverviewPage.vue";
import HelpGettingStartedPage from "./views/HelpGettingStartedPage.vue";
import HelpConceptsPage from "./views/HelpConceptsPage.vue";
import HelpHowToOpenPage from "./views/HelpHowToOpenPage.vue";
import HelpHowToCreatePage from "./views/HelpHowToCreatePage.vue";
import HelpHowToImportPage from "./views/HelpHowToImportPage.vue";
import HelpFaqPage from "./views/HelpFaqPage.vue";
import ApiOverviewPage from "./views/ApiOverviewPage.vue";
import ApiNamespacesPage from "./views/ApiNamespacesPage.vue";
import ApiNamespacePage from "./views/ApiNamespacePage.vue";
import ApiTypeDetailPage from "./views/ApiTypeDetailPage.vue";

const router = createRouter({
  history: createWebHashHistory(),
  routes: [
    { path: "/", component: HomePage },
    { path: "/toc", component: TocPage },
    { path: "/help", component: HelpOverviewPage },
    { path: "/help/getting-started", component: HelpGettingStartedPage },
    { path: "/help/concepts", component: HelpConceptsPage },
    { path: "/help/how-to/open-and-read", component: HelpHowToOpenPage },
    { path: "/help/how-to/create-folder-and-message", component: HelpHowToCreatePage },
    { path: "/help/how-to/import-eml", component: HelpHowToImportPage },
    { path: "/help/faq", component: HelpFaqPage },
    { path: "/api", component: ApiOverviewPage },
    { path: "/api/namespaces", component: ApiNamespacesPage },
    { path: "/api/namespace/:name", component: ApiNamespacePage, props: true },
    { path: "/api/type/:namespace/:type", component: ApiTypeDetailPage, props: true }
  ]
});

export default router;
