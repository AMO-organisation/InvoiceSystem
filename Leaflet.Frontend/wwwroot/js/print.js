window.leaflet = {
    print: function () {
        window.print();
    },
    clampContextMenu: function (element) {
        if (!element) {
            return;
        }

        const margin = 12;
        const rect = element.getBoundingClientRect();
        let left = rect.left;
        let top = rect.top;

        if (rect.right > window.innerWidth - margin) {
            left = window.innerWidth - rect.width - margin;
        }

        if (rect.bottom > window.innerHeight - margin) {
            top = window.innerHeight - rect.height - margin;
        }

        element.style.left = Math.max(margin, left) + "px";
        element.style.top = Math.max(margin, top) + "px";
    }
};
