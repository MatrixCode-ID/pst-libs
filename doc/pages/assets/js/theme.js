(function () {
  var storageKey = "doc-theme";
  var root = document.documentElement;
  var prefersDark = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;
  var saved = localStorage.getItem(storageKey);
  var theme = (saved === "dark" || saved === "light") ? saved : (prefersDark ? "dark" : "light");

  function applyTheme(value) {
    root.setAttribute("data-theme", value);
    localStorage.setItem(storageKey, value);

    var button = document.getElementById("doc-theme-toggle");
    if (button) {
      button.textContent = value === "dark" ? "Light mode" : "Dark mode";
      button.setAttribute("aria-label", "Switch theme to " + (value === "dark" ? "light" : "dark"));
    }
  }

  function addToggle() {
    var main = document.querySelector("main");
    if (!main || document.getElementById("doc-theme-toggle")) {
      return;
    }

    var bar = document.createElement("div");
    bar.className = "doc-theme-bar";

    var button = document.createElement("button");
    button.id = "doc-theme-toggle";
    button.type = "button";
    button.addEventListener("click", function () {
      var next = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
      applyTheme(next);
    });

    bar.appendChild(button);
    main.insertBefore(bar, main.firstChild);
  }

  applyTheme(theme);

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", function () {
      addToggle();
      applyTheme(root.getAttribute("data-theme") || theme);
    });
  } else {
    addToggle();
    applyTheme(root.getAttribute("data-theme") || theme);
  }
})();
