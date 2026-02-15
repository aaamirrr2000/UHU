// Makes a modal draggable by its title bar. Call with the id of the .modal-content element.
window.makeModalDraggable = function (contentId) {
    var content = document.getElementById(contentId);
    if (!content || content._dragSetup) return;
    content._dragSetup = true;

    var titleBar = content.querySelector('.modal-title');
    if (!titleBar) return;

    // Ensure content can be positioned
    content.style.position = 'fixed';
    content.style.left = '50%';
    content.style.top = '50%';
    content.style.transform = 'translate(-50%, -50%)';
    content.style.margin = '0';

    var startX, startY, startLeft, startTop;

    titleBar.addEventListener('mousedown', function (e) {
        if (e.button !== 0) return;
        e.preventDefault();
        var rect = content.getBoundingClientRect();
        startX = e.clientX;
        startY = e.clientY;
        startLeft = rect.left;
        startTop = rect.top;
        content.style.transform = 'none';
        content.style.left = startLeft + 'px';
        content.style.top = startTop + 'px';
        content._dragging = true;
    });

    document.addEventListener('mousemove', function (e) {
        if (!content._dragging) return;
        var dx = e.clientX - startX;
        var dy = e.clientY - startY;
        startLeft += dx;
        startTop += dy;
        startX = e.clientX;
        startY = e.clientY;
        content.style.left = startLeft + 'px';
        content.style.top = startTop + 'px';
    });

    document.addEventListener('mouseup', function () {
        content._dragging = false;
    });

    titleBar.style.cursor = 'move';
    titleBar.style.userSelect = 'none';
};
