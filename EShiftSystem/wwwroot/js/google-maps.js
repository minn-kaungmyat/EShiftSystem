// Google Maps integration for EShiftSystem
let map;
let startMarker;
let endMarker;
let directionsService;
let directionsRenderer;

// Initialize Google Maps
function initGoogleMaps() {
    console.log('Google Maps API loaded successfully');
    
    // Initialize autocomplete for address fields
    initializeAutocomplete();
    
    // Initialize map if we're on a details page
    if (document.getElementById('map')) {
        initializeMap();
    }
}

// Initialize autocomplete for address input fields
function initializeAutocomplete() {
    const startLocationInput = document.getElementById('StartLocation');
    const destinationInput = document.getElementById('Destination');
    
    if (startLocationInput) {
        const startAutocomplete = new google.maps.places.Autocomplete(startLocationInput, {
            types: ['geocode', 'establishment'],
            componentRestrictions: { country: 'th' } // Thailand
        });
        
        startAutocomplete.addListener('place_changed', function() {
            const place = startAutocomplete.getPlace();
            if (place.geometry) {
                updateLocationCoordinates('start', place);
                updateMapIfExists();
            }
        });
    }
    
    if (destinationInput) {
        const destinationAutocomplete = new google.maps.places.Autocomplete(destinationInput, {
            types: ['geocode', 'establishment'],
            componentRestrictions: { country: 'th' } // Thailand
        });
        
        destinationAutocomplete.addListener('place_changed', function() {
            const place = destinationAutocomplete.getPlace();
            if (place.geometry) {
                updateLocationCoordinates('destination', place);
                updateMapIfExists();
            }
        });
    }
}

// Update hidden coordinate fields
function updateLocationCoordinates(type, place) {
    const lat = place.geometry.location.lat();
    const lng = place.geometry.location.lng();
    
    if (type === 'start') {
        const latField = document.getElementById('StartLatitude');
        const lngField = document.getElementById('StartLongitude');
        if (latField) latField.value = lat;
        if (lngField) lngField.value = lng;
    } else if (type === 'destination') {
        const latField = document.getElementById('DestinationLatitude');
        const lngField = document.getElementById('DestinationLongitude');
        if (latField) latField.value = lat;
        if (lngField) lngField.value = lng;
    }
}

// Initialize map for details view
function initializeMap() {
    const mapElement = document.getElementById('map');
    if (!mapElement) return;
    
    // Default center - Bangkok, Thailand
    const defaultCenter = { lat: 13.7563, lng: 100.5018 }; // Bangkok, Thailand
    
    map = new google.maps.Map(mapElement, {
        zoom: 11,
        center: defaultCenter,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        disableDefaultUI: true, // Minimalist approach
        zoomControl: true,
        mapTypeControl: false,
        scaleControl: false,
        streetViewControl: false,
        rotateControl: false,
        fullscreenControl: false
    });
    
    directionsService = new google.maps.DirectionsService();
    directionsRenderer = new google.maps.DirectionsRenderer({
        draggable: false,
        suppressMarkers: false,
        suppressInfoWindows: true // Minimalist approach
    });
    directionsRenderer.setMap(map);
    
    // Try to get user's current location
    getUserLocationAndCenter();
    
    // Load existing coordinates if available
    loadExistingCoordinates();
}

// Get user's current location and center map
function getUserLocationAndCenter() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            function(position) {
                const userLocation = {
                    lat: position.coords.latitude,
                    lng: position.coords.longitude
                };
                
                // Only center on user location if in Thailand (rough bounds check)
                if (userLocation.lat >= 5.6 && userLocation.lat <= 20.5 && 
                    userLocation.lng >= 97.3 && userLocation.lng <= 105.6) {
                    map.setCenter(userLocation);
                    map.setZoom(13);
                    
                    // Add a subtle marker for user location
                    new google.maps.Marker({
                        position: userLocation,
                        map: map,
                        title: 'Your Location',
                        icon: {
                            url: 'https://maps.google.com/mapfiles/ms/icons/blue-dot.png',
                            scaledSize: new google.maps.Size(20, 20)
                        }
                    });
                }
            },
            function(error) {
                console.log('Geolocation error:', error.message);
                // Fallback to Bangkok center - already set
            },
            {
                timeout: 5000,
                enableHighAccuracy: false
            }
        );
    }
}

// Load existing coordinates from the page data
function loadExistingCoordinates() {
    // First try data attributes (for details pages)
    let startLat = document.querySelector('[data-start-lat]')?.getAttribute('data-start-lat');
    let startLng = document.querySelector('[data-start-lng]')?.getAttribute('data-start-lng');
    let destLat = document.querySelector('[data-dest-lat]')?.getAttribute('data-dest-lat');
    let destLng = document.querySelector('[data-dest-lng]')?.getAttribute('data-dest-lng');
    
    // If no data attributes, try hidden form fields (for edit/create forms)
    if (!startLat || !startLng || !destLat || !destLng) {
        startLat = document.getElementById('StartLatitude')?.value;
        startLng = document.getElementById('StartLongitude')?.value;
        destLat = document.getElementById('DestinationLatitude')?.value;
        destLng = document.getElementById('DestinationLongitude')?.value;
    }
    
    if (startLat && startLng && destLat && destLng) {
        const start = new google.maps.LatLng(parseFloat(startLat), parseFloat(startLng));
        const destination = new google.maps.LatLng(parseFloat(destLat), parseFloat(destLng));
        
        displayRoute(start, destination);
    } else {
        // If no coordinates, try to geocode the addresses
        let startAddress = document.querySelector('[data-start-address]')?.getAttribute('data-start-address');
        let destAddress = document.querySelector('[data-dest-address]')?.getAttribute('data-dest-address');
        
        // If no data attributes, try form fields
        if (!startAddress || !destAddress) {
            startAddress = document.getElementById('StartLocation')?.value;
            destAddress = document.getElementById('Destination')?.value;
        }
        
        if (startAddress && destAddress) {
            geocodeAndDisplayRoute(startAddress, destAddress);
        }
    }
}

// Display route between two points
function displayRoute(start, destination) {
    const request = {
        origin: start,
        destination: destination,
        travelMode: google.maps.TravelMode.DRIVING
    };
    
    directionsService.route(request, function(result, status) {
        if (status === 'OK') {
            directionsRenderer.setDirections(result);
        } else {
            console.error('Directions request failed due to ' + status);
            // Fallback: show markers instead
            showMarkersOnly(start, destination);
        }
    });
}

// Geocode addresses and display route
function geocodeAndDisplayRoute(startAddress, destAddress) {
    const geocoder = new google.maps.Geocoder();
    
    geocoder.geocode({ address: startAddress }, function(startResults, startStatus) {
        if (startStatus === 'OK') {
            geocoder.geocode({ address: destAddress }, function(destResults, destStatus) {
                if (destStatus === 'OK') {
                    const start = startResults[0].geometry.location;
                    const destination = destResults[0].geometry.location;
                    displayRoute(start, destination);
                } else {
                    console.error('Destination geocoding failed: ' + destStatus);
                }
            });
        } else {
            console.error('Start geocoding failed: ' + startStatus);
        }
    });
}

// Show markers only (fallback when directions fail)
function showMarkersOnly(start, destination) {
    if (startMarker) startMarker.setMap(null);
    if (endMarker) endMarker.setMap(null);
    
    startMarker = new google.maps.Marker({
        position: start,
        map: map,
        title: 'Start Location',
        icon: {
            url: 'https://maps.google.com/mapfiles/ms/icons/green-dot.png'
        }
    });
    
    endMarker = new google.maps.Marker({
        position: destination,
        map: map,
        title: 'Destination',
        icon: {
            url: 'https://maps.google.com/mapfiles/ms/icons/red-dot.png'
        }
    });
    
    // Fit map to show both markers
    const bounds = new google.maps.LatLngBounds();
    bounds.extend(start);
    bounds.extend(destination);
    map.fitBounds(bounds);
}

// Update map when addresses change (for create/edit forms)
function updateMapIfExists() {
    if (!map) return;
    
    const startLat = document.getElementById('StartLatitude')?.value;
    const startLng = document.getElementById('StartLongitude')?.value;
    const destLat = document.getElementById('DestinationLatitude')?.value;
    const destLng = document.getElementById('DestinationLongitude')?.value;
    
    if (startLat && startLng && destLat && destLng) {
        const start = new google.maps.LatLng(parseFloat(startLat), parseFloat(startLng));
        const destination = new google.maps.LatLng(parseFloat(destLat), parseFloat(destLng));
        displayRoute(start, destination);
    }
}

// Utility function to create a map for a specific container
function createMapForContainer(containerId, startLat, startLng, destLat, destLng) {
    const mapContainer = document.getElementById(containerId);
    if (!mapContainer) return;
    
    const mapInstance = new google.maps.Map(mapContainer, {
        zoom: 10,
        center: { lat: (parseFloat(startLat) + parseFloat(destLat)) / 2, lng: (parseFloat(startLng) + parseFloat(destLng)) / 2 },
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });
    
    const directionsServiceInstance = new google.maps.DirectionsService();
    const directionsRendererInstance = new google.maps.DirectionsRenderer();
    directionsRendererInstance.setMap(mapInstance);
    
    const start = new google.maps.LatLng(parseFloat(startLat), parseFloat(startLng));
    const destination = new google.maps.LatLng(parseFloat(destLat), parseFloat(destLng));
    
    const request = {
        origin: start,
        destination: destination,
        travelMode: google.maps.TravelMode.DRIVING
    };
    
    directionsServiceInstance.route(request, function(result, status) {
        if (status === 'OK') {
            directionsRendererInstance.setDirections(result);
        }
    });
}

// Function to refresh map with current form data (useful for edit forms)
function refreshMapWithFormData() {
    if (!map) return;
    
    // Small delay to ensure form fields are populated
    setTimeout(() => {
        loadExistingCoordinates();
    }, 100);
}

// Make functions globally available
window.initGoogleMaps = initGoogleMaps;
window.createMapForContainer = createMapForContainer;
window.refreshMapWithFormData = refreshMapWithFormData; 