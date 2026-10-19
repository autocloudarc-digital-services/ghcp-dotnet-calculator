export function attach(reference) {
    let keyboardEntry = false;
    const resetKeyboardEntry = () => { keyboardEntry = false; };
    const handler = (event) => {
        if (event.ctrlKey || event.metaKey || event.altKey || event.isComposing) return;
        if (event.key === "Tab") {
            resetKeyboardEntry();
            return;
        }
        const target = event.target;
        if (target instanceof Element) {
            if (target.closest("input, textarea, select, [contenteditable]")) return;
            // Preserve native Enter activation after Tab navigation, but calculate after typing.
            if (event.key === "Enter" && !keyboardEntry && target.closest("button, a")) return;
        }

        const key = event.key;
        if (!/^[0-9.+*/%^\-=Cc]$/.test(key) &&
            !["Enter", "Backspace", "Delete"].includes(key)) return;

        event.preventDefault();
        keyboardEntry = true;
        reference.invokeMethodAsync("HandleKeyboard", key)
            .catch((error) => console.error("Calculator keyboard input failed.", error));
    };

    window.addEventListener("keydown", handler);
    window.addEventListener("pointerdown", resetKeyboardEntry);
    return DotNet.createJSObjectReference({
        dispose: () => {
            window.removeEventListener("keydown", handler);
            window.removeEventListener("pointerdown", resetKeyboardEntry);
        }
    });
}
