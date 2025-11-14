window.idleHandler = {
    startIdleTimer: function (dotNetHelper) {
        let idleTime = 0;
        const refreshAfter = 5 * 60;   // 5 minutes
        const logoutAfter = 10 * 60;  // 15 minutes

        function resetTimer() {
            idleTime = 0;
        }

        // reset on user activity
        ['mousemove', 'keypress', 'scroll', 'click'].forEach(evt =>
            document.addEventListener(evt, resetTimer)
        );

        setInterval(() => {
            idleTime++;
            dotNetHelper.invokeMethodAsync("UpdateIdleTime", idleTime);

            if (idleTime === refreshAfter) {
                dotNetHelper.invokeMethodAsync("RefreshPage");
            }
            if (idleTime >= logoutAfter) {
                dotNetHelper.invokeMethodAsync("LogoutUser");
            }
        }, 1000);
    }
};
