(function () {
    function openDatabase(databaseName, storeName) {
        return new Promise(function (resolve, reject) {
            var request = indexedDB.open(databaseName);

            request.onupgradeneeded = function (event) {
                var db = event.target.result;
                if (!db.objectStoreNames.contains(storeName)) {
                    db.createObjectStore(storeName, { keyPath: "Id" });
                }
            };

            request.onsuccess = function (event) {
                var db = event.target.result;
                if (!db.objectStoreNames.contains(storeName)) {
                    db.close();
                    var upgradeRequest = indexedDB.open(databaseName, db.version + 1);
                    upgradeRequest.onupgradeneeded = function (upgradeEvent) {
                        var upgradedDb = upgradeEvent.target.result;
                        if (!upgradedDb.objectStoreNames.contains(storeName)) {
                            upgradedDb.createObjectStore(storeName, { keyPath: "Id" });
                        }
                    };
                    upgradeRequest.onsuccess = function (upgradeEvent) {
                        resolve(upgradeEvent.target.result);
                    };
                    upgradeRequest.onerror = function (upgradeEvent) {
                        reject(upgradeEvent.target.error || new Error("Failed to upgrade IndexedDB."));
                    };
                    return;
                }

                resolve(db);
            };

            request.onerror = function (event) {
                reject(event.target.error || new Error("Failed to open IndexedDB."));
            };
        });
    }

    function runReadWrite(databaseName, storeName, action) {
        return openDatabase(databaseName, storeName).then(function (db) {
            return new Promise(function (resolve, reject) {
                var transaction = db.transaction(storeName, "readwrite");
                var store = transaction.objectStore(storeName);

                transaction.oncomplete = function () {
                    db.close();
                    resolve();
                };
                transaction.onerror = function (event) {
                    db.close();
                    reject(event.target.error || new Error("IndexedDB transaction failed."));
                };

                action(store, reject);
            });
        });
    }

    function runReadOnly(databaseName, storeName, action) {
        return openDatabase(databaseName, storeName).then(function (db) {
            return new Promise(function (resolve, reject) {
                var transaction = db.transaction(storeName, "readonly");
                var store = transaction.objectStore(storeName);

                action(store, resolve, reject);

                transaction.onerror = function (event) {
                    db.close();
                    reject(event.target.error || new Error("IndexedDB transaction failed."));
                };
                transaction.oncomplete = function () {
                    db.close();
                };
            });
        });
    }

    window.crohnsDiaryIndexedDb = {
        add: function (databaseName, storeName, item) {
            return runReadWrite(databaseName, storeName, function (store, reject) {
                var request = store.add(item);
                request.onerror = function (event) {
                    reject(event.target.error || new Error("Failed to add item."));
                };
            });
        },

        bulkPut: function (databaseName, storeName, items) {
            return runReadWrite(databaseName, storeName, function (store, reject) {
                (items || []).forEach(function (item) {
                    var request = store.put(item);
                    request.onerror = function (event) {
                        reject(event.target.error || new Error("Failed to write item."));
                    };
                });
            });
        },

        getAll: function (databaseName, storeName) {
            return runReadOnly(databaseName, storeName, function (store, resolve, reject) {
                var request = store.getAll();
                request.onsuccess = function () {
                    resolve(request.result || []);
                };
                request.onerror = function (event) {
                    reject(event.target.error || new Error("Failed to read items."));
                };
            });
        }
    };
})();
