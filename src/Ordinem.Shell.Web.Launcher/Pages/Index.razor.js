window.open(
	"www.google.com", // Window position and dimensions supplied via query params
	"popup-1",
	"noopener"
);
window.ShowAlert = (message) => {
	alert(message);
}