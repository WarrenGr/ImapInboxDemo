"use strict";

document.addEventListener("DOMContentLoaded", function () {

    const phaseEl = document.getElementById("phase");
    const processedEl = document.getElementById("processed");
    const insertedEl = document.getElementById("inserted");
    const totalEl = document.getElementById("total");
    const uidEl = document.getElementById("currentUid");
    const mpsEl = document.getElementById("mps");
    const etaEl = document.getElementById("eta");
    const lastUpdatedEl = document.getElementById("lastUpdated");
    const progressBar = document.getElementById("progressBar");

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/imapsynchub")
        .withAutomaticReconnect()
        .build();

    connection.on("syncProgress", (data) => {
        phaseEl.textContent = data.phase;
        processedEl.textContent = data.processed;
        insertedEl.textContent = data.inserted;
        totalEl.textContent = data.totalMessages;
        uidEl.textContent = data.currentUid;
        mpsEl.textContent = data.messagesPerSecond.toFixed(2);

        etaEl.textContent = data.etaSeconds > 0
            ? Math.round(data.etaSeconds) + "s"
            : "0s";

        lastUpdatedEl.textContent = new Date(data.lastUpdated).toLocaleTimeString();

        // Smooth progress bar
        const pct = data.totalMessages > 0
            ? (data.processed / data.totalMessages) * 100
            : 0;

        progressBar.style.width = pct.toFixed(1) + "%";
    });

    connection.start().catch(err => console.error(err));
});