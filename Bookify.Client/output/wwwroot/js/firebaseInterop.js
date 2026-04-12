window.firebaseInterop = {
    initFirebase: function (config) {
        // TODO: Initialize Firebase with the provided config from blazor
        console.log("Firebase init stub called with config");
        return true;
    },
    requestPermission: function () {
        // TODO: Request notification permission from browser
        console.log("Firebase request permission stub called");
        return true;
    },
    getToken: function () {
        // TODO: Get FCM token to send back to server
        console.log("Firebase get token stub called");
        return "stub-fcm-token-123";
    }
};
