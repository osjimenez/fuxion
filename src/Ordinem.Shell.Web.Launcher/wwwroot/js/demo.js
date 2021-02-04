export function showPrompt(url) {
	window.open(
		url, // Window position and dimensions supplied via query params
		"_blank ",
		"resizable=yes,menubar=no,location=no,titlebar=no,toolbar=no"
	);
}