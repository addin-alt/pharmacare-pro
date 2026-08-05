(() => {
    "use strict";

    function bindPasswordToggles() {
        document
            .querySelectorAll("[data-password-toggle]")
            .forEach((button) => {
                if (button.dataset.authBound === "true") {
                    return;
                }

                button.dataset.authBound = "true";

                button.addEventListener("click", () => {
                    const targetId =
                        button.getAttribute("data-target");

                    const input =
                        document.getElementById(targetId);

                    if (!input) {
                        return;
                    }

                    const shouldShow =
                        input.type === "password";

                    input.type =
                        shouldShow ? "text" : "password";

                    button.setAttribute(
                        "aria-pressed",
                        shouldShow.toString());

                    const label =
                        button.querySelector(
                            "[data-toggle-label]");

                    if (label) {
                        label.textContent =
                            shouldShow ? "Hide" : "Show";
                    }
                });
            });
    }

    function getPasswordRules(value) {
        return {
            length: value.length >= 10,
            upper: /[A-Z]/.test(value),
            lower: /[a-z]/.test(value),
            number: /\d/.test(value),
            symbol: /[^A-Za-z0-9]/.test(value)
        };
    }

    function updatePasswordGuide(input) {
        const checklist =
            document.querySelector(
                `[data-password-checklist="${input.id}"]`);

        if (!checklist) {
            return;
        }

        const rules =
            getPasswordRules(input.value);

        let completed = 0;

        Object.entries(rules).forEach(
            ([ruleName, isValid]) => {
                const rule =
                    checklist.querySelector(
                        `[data-rule="${ruleName}"]`);

                if (!rule) {
                    return;
                }

                rule.classList.toggle(
                    "is-valid",
                    isValid);

                if (isValid) {
                    completed++;
                }
            });

        const progress =
            checklist.querySelector(
                "[data-password-strength]");

        const strengthLabel =
            checklist.querySelector(
                "[data-strength-label]");

        if (progress) {
            progress.style.width =
                `${completed * 20}%`;

            progress.dataset.level =
                completed <= 2
                    ? "weak"
                    : completed <= 4
                        ? "medium"
                        : "strong";
        }

        if (strengthLabel) {
            if (input.value.length === 0) {
                strengthLabel.textContent =
                    "Start typing";
            }
            else if (completed <= 2) {
                strengthLabel.textContent =
                    "Needs improvement";
            }
            else if (completed <= 4) {
                strengthLabel.textContent =
                    "Almost ready";
            }
            else {
                strengthLabel.textContent =
                    "Strong password";
            }
        }

        updatePasswordMatch();
    }

    function updatePasswordMatch() {
        document
            .querySelectorAll("[data-password-match]")
            .forEach((element) => {
                const passwordId =
                    element.getAttribute(
                        "data-password-source");

                const confirmId =
                    element.getAttribute(
                        "data-confirm-source");

                const password =
                    document.getElementById(passwordId);

                const confirmation =
                    document.getElementById(confirmId);

                if (!password || !confirmation) {
                    return;
                }

                const isMatch =
                    confirmation.value.length > 0 &&
                    password.value === confirmation.value;

                element.classList.toggle(
                    "is-visible",
                    confirmation.value.length > 0);

                element.classList.toggle(
                    "is-valid",
                    isMatch);

                const text =
                    element.querySelector("p");

                if (text) {
                    text.textContent =
                        isMatch
                            ? "Passwords match"
                            : "Passwords do not match yet";
                }
            });
    }

    function bindPasswordGuides() {
        document
            .querySelectorAll("[data-auth-password]")
            .forEach((input) => {
                if (input.dataset.guideBound === "true") {
                    return;
                }

                input.dataset.guideBound = "true";

                input.addEventListener(
                    "input",
                    () => updatePasswordGuide(input));

                updatePasswordGuide(input);
            });

        document
            .querySelectorAll("[data-confirm-password]")
            .forEach((input) => {
                if (input.dataset.confirmBound === "true") {
                    return;
                }

                input.dataset.confirmBound = "true";

                input.addEventListener(
                    "input",
                    updatePasswordMatch);
            });
    }

    function initializeAuthenticationUi() {
        bindPasswordToggles();
        bindPasswordGuides();
        updatePasswordMatch();
    }

    document.addEventListener(
        "DOMContentLoaded",
        initializeAuthenticationUi);

    document.addEventListener(
        "enhancedload",
        initializeAuthenticationUi);
})();
