const pages = {
    "profile": "accountview.html",
    "settings": "settings.html",
    "progress": "progress.html",
    "certificates": "certifications.html",
    "certifications": "certifications.html",
    "password": "settings.html"
};

function searchPage() {
    let searchQuery = document.getElementById("searchInput").value.toLowerCase().trim();
    if (pages[searchQuery]) {
        window.location.href = pages[searchQuery];
    } else {
        alert("Page not found!");
    }
}

async function fetchUserProfile() {
    const token = sessionStorage.getItem("token") || localStorage.getItem("token");
    if (!token) {
        alert("Session expired, please log in again.");
        window.location.href = "../signin.html";
        return;
    }

    try {
        const response = await fetch("http://localhost:5147/api/auth/profile", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) {
            const errorData = await response.json();
            console.error("Server error:", errorData);
            alert(errorData.message || "Failed to fetch user profile");
            return;
        }

        const data = await response.json();
        console.log("User profile data:", data);

        const userNameEl = document.getElementById("user-name");
        const userCardNameEl = document.getElementById("user-card-name");

        if (userNameEl) userNameEl.textContent = data.username || "Guest";
        if (userCardNameEl && userCardNameEl.childNodes[0])
            userCardNameEl.childNodes[0].textContent = data.username + " ";

    } catch (error) {
        console.error("Error fetching user profile:", error);
        alert("Error loading profile.");
    }
}


// the user data fetch in the profile welcome coloumn
document.getElementById('userLABS').textContent = userData.UserLabs || "Guest";
document.getElementById('userLEVEL').textContent = userData.UserLevel || "Guest";




async function loadUserData() {
    const token = sessionStorage.getItem("token") || localStorage.getItem("token");

    if (!token) {
        alert("Unauthorized! Please log in.");
        window.location.href = "../signin.html";
        return;
    }

    try {
        const response = await fetch("http://localhost:5147/api/auth/profile", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        if (!response.ok) throw new Error("Failed to fetch user data!");

        const userData = await response.json();
        console.log("📦 userData:", userData);


        document.getElementById("Username").value = userData.username || "";
        document.getElementById("Email").value = userData.email || "";
        document.getElementById("Phone").value = userData.phoneNumber || "";



    } catch (error) {
        console.error("❌ Error fetching user data:", error);
    }
}

async function saveChanges() {
    const token = sessionStorage.getItem("token") || localStorage.getItem("token");
    if (!token) {
        alert("Unauthorized! Please log in.");
        return;
    }

    const username = document.getElementById("Username").value.trim();
    const email = document.getElementById("Email").value.trim();
    const phone = document.getElementById("Phone").value.trim();

    try {
        const response = await fetch("http://localhost:5147/api/auth/update-profile", {
            method: "PUT",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ Username: username, Email: email, PhoneNumber: phone })
        });

        const result = await response.json();

        if (!response.ok) throw new Error(result.message || "Failed to update profile");

        alert("✅ Profile updated successfully!");

        // تحديث القيم الظاهرة بعد الحفظ
        document.getElementById("Username").value = result.username;
        document.getElementById("Email").value = result.email;
        document.getElementById("Phone").value = result.phoneNumber;

        // إرجاع الحقول لوضع القراءة فقط (اختياري)
        document.getElementById("Username").disabled = true;
        document.getElementById("Email").disabled = true;
        document.getElementById("Phone").disabled = true;

    } catch (error) {
        console.error("Error updating profile:", error);
        alert("⚠️ Error updating profile.");
    }
}


async function submitNewPassword() {
    const oldPass = document.getElementById("oldPassword").value;
    const newPass = document.getElementById("newPassword").value;
    const confirmPass = document.getElementById("confirmPassword").value;
    const errorMessage = document.getElementById("ERRORmessage");
    const token = sessionStorage.getItem("token") || localStorage.getItem("token");

    errorMessage.style.display = "none";

    if (!oldPass || !newPass || !confirmPass) {
        errorMessage.textContent = "Please fill in all fields.";
        errorMessage.style.display = "block";
        return;
    }

    if (newPass !== confirmPass) {
        errorMessage.textContent = "New passwords do not match.";
        errorMessage.style.display = "block";
        return;
    }

    try {
        const response = await fetch("http://localhost:5147/api/auth/change-password", {
            method: "PUT",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                oldPassword: oldPass,
                newPassword: newPass
            })
        });

        const result = await response.json();

        if (!response.ok) throw new Error(result.message || "Failed to change password.");

        alert("Password changed successfully!");
        document.getElementById("oldPassword").value = "";
        document.getElementById("newPassword").value = "";
        document.getElementById("confirmPassword").value = "";
        enablePasswordInputs(false);

    } catch (error) {
        errorMessage.textContent = error.message;
        errorMessage.style.display = "block";
    }
}

function enableEdit(inputId) {
    let inputField = document.getElementById(inputId);
    inputField.removeAttribute("disabled");
    inputField.focus();

    inputField.onblur = () => {
        saveData(inputId, inputField.value);
        inputField.setAttribute("disabled", true);
    };

    inputField.onkeydown = (event) => {
        if (event.key === "Enter") inputField.blur();
    };
}

function enablePasswordInputs(enable = true) {
    document.getElementById("oldPassword").disabled = !enable;
    document.getElementById("newPassword").disabled = !enable;
    document.getElementById("confirmPassword").disabled = !enable;
}

function saveData(inputId, value) {
    console.log(`Saving ${inputId}: ${value}`);
}

function logoutUser() {
    localStorage.removeItem("token");
    sessionStorage.removeItem("token");
    window.location.href = "../signin.html";
}

function initializeNotifications() {
    let notificationBadge = document.getElementById("notificationBadge");
    let notificationIcon = document.getElementById("notificationIcon");
    let notificationList = document.getElementById("notificationList");
    let notificationsUl = document.getElementById("notifications");
    let notifications = [];

    setTimeout(() => addNotification("New message from the Founder!"), 3000);
    setTimeout(() => addNotification("Your progress has been updated."), 5000);

    function addNotification(message) {
        notifications.push(message);
        updateNotificationsUI();
    }

    function updateNotificationsUI() {
        notificationsUl.innerHTML = "";
        notifications.forEach((msg) => {
            let li = document.createElement("li");
            li.textContent = msg;
            notificationsUl.appendChild(li);
        });
        notifications.length > 0 ? showNotification() : hideNotification();
    }

    function showNotification() {
        notificationBadge.style.display = "block";
    }

    function hideNotification() {
        notificationBadge.style.display = "none";
    }

    notificationIcon.addEventListener("click", (e) => {
        e.stopPropagation();
        hideNotification();
        notificationList.style.display = (notificationList.style.display === "block") ? "none" : "block";
    });

    document.addEventListener("click", (event) => {
        if (!notificationIcon.contains(event.target) && !notificationList.contains(event.target)) {
            notificationList.style.display = "none";
        }
    });
}




document.addEventListener("DOMContentLoaded", () => {
    fetch('/api/labs')
        .then(response => response.json())
        .then(data => {
            const tableBody = document.getElementById('labs-table-body');
            data.forEach(item => {
                const row = document.createElement('tr');

                row.innerHTML = `
              <td>${item.lab}</td>
              <td>${item.date}</td>
              <td>${item.score}</td>
              <td>${item.status}</td>
            `;

                tableBody.appendChild(row);
            });
        })
        .catch(error => {
            console.error('Error fetching lab data:', error);
        });
});




document.addEventListener("DOMContentLoaded", function () {
    fetchUserProfile();
    loadUserData();
    initializeNotifications();

    const logoutBtn = document.getElementById("logoutButton");
    if (logoutBtn) logoutBtn.addEventListener("click", logoutUser);

    const searchInput = document.getElementById("searchInput");
    if (searchInput) {
        searchInput.addEventListener("keypress", function (event) {
            if (event.key === "Enter") searchPage();
        });
    }

    const searchBtn = document.getElementById("searchButton");
    if (searchBtn) searchBtn.addEventListener("click", searchPage);
});









