export function downloadTextFile(fileName, content, contentType) {
  const blob = new Blob([content], {
    type: contentType || "text/plain;charset=utf-8",
  });
  const url = URL.createObjectURL(blob);

  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName || "download.txt";
  anchor.style.display = "none";

  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();

  URL.revokeObjectURL(url);
}

export function getLocalState(key) {
  return window.localStorage.getItem(key);
}

export function setLocalState(key, value) {
  window.localStorage.setItem(key, value);
}

export function removeLocalState(key) {
  window.localStorage.removeItem(key);
}
