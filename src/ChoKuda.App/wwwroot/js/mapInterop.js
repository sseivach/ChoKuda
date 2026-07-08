const maps = new Map();

export function initMap(elementId, apiKey, dotNetReference) {
    const element = document.getElementById(elementId);

    if (!element) {
        throw new Error(`Map element not found: ${elementId}`);
    }

    const map = L.map(element, {
        center: [39.5, -98.35],
        zoom: 4,
        minZoom: 3,
    });

    const tileUrl = `https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png?api_key=${encodeURIComponent(apiKey)}`;
    const tileLayer = L.tileLayer(tileUrl, {
        maxZoom: 20,
        detectRetina: true,
        attribution: '&copy; <a href="https://stadiamaps.com/">Stadia Maps</a> &copy; <a href="https://openmaptiles.org/">OpenMapTiles</a> &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap contributors</a>',
    });

    tileLayer.addTo(map);

    const clusterGroup = L.markerClusterGroup({
        showCoverageOnHover: false,
        zoomToBoundsOnClick: false,
    });

    clusterGroup.on('clusterclick', (event) => {
        const size = map.getSize();
        const padding = L.point(size.x * 0.1, size.y * 0.1);
        map.fitBounds(event.layer.getBounds(), {
            paddingTopLeft: padding,
            paddingBottomRight: padding,
        });
    });

    clusterGroup.addTo(map);

    map.on('click', (event) => {
        dotNetReference.invokeMethodAsync(
            'HandleMapClick',
            event.latlng.lat,
            event.latlng.lng);
    });

    const escapeHandler = (event) => {
        if (event.key === 'Escape') {
            dotNetReference.invokeMethodAsync('HandleEscape');
        }
    };

    window.addEventListener('keydown', escapeHandler);

    maps.set(elementId, {
        map,
        clusterGroup,
        markers: [],
        temporaryMarker: null,
        escapeHandler,
        dotNetReference,
    });
}

export function setPoints(elementId, points) {
    const state = getMapState(elementId);
    state.clusterGroup.clearLayers();
    state.markers = [];

    for (const point of points ?? []) {
        const marker = L.marker([point.latitude, point.longitude], {
            icon: createPointIcon(point.pinIconId, point.pinColor),
            title: point.title,
        });

        marker.bindTooltip(point.title, {
            direction: 'top',
            offset: [0, -18],
        });

        marker.on('click', () => {
            state.dotNetReference?.invokeMethodAsync('HandlePointClick', point.id);
        });

        state.markers.push(marker);
        state.clusterGroup.addLayer(marker);
    }
}

export function setTemporaryMarker(elementId, latitude, longitude) {
    const state = getMapState(elementId);
    const latLng = [latitude, longitude];

    if (state.temporaryMarker) {
        state.temporaryMarker.setLatLng(latLng);
        return;
    }

    state.temporaryMarker = L.marker(latLng, {
        icon: createTemporaryIcon(),
        title: 'Selected coordinates',
        zIndexOffset: 1000,
    });

    state.temporaryMarker.addTo(state.map);
}

export function clearTemporaryMarker(elementId) {
    const state = getMapState(elementId);

    if (!state.temporaryMarker) {
        return;
    }

    state.temporaryMarker.remove();
    state.temporaryMarker = null;
}

export function disposeMap(elementId) {
    const state = maps.get(elementId);

    if (!state) {
        return;
    }

    if (state.escapeHandler) {
        window.removeEventListener('keydown', state.escapeHandler);
    }

    state.map.remove();
    maps.delete(elementId);
}

function getMapState(elementId) {
    const state = maps.get(elementId);

    if (!state) {
        throw new Error(`Map is not initialized: ${elementId}`);
    }

    return state;
}

function createPointIcon(iconId, color) {
    const safeIconId = (iconId || 'geo-alt-fill').replace(/[^a-z0-9-]/g, '');
    const safeColor = color || '#2f75b5';

    return L.divIcon({
        className: 'chokuda-map-pin',
        html: `<span style="--pin-color: ${safeColor}"><i class="bi bi-${safeIconId}"></i></span>`,
        iconSize: [28, 36],
        iconAnchor: [14, 34],
        popupAnchor: [0, -30],
    });
}

function createTemporaryIcon() {
    return L.divIcon({
        className: 'chokuda-temporary-pin',
        html: '<span></span>',
        iconSize: [28, 36],
        iconAnchor: [14, 34],
    });
}
