
package com.nostradabus.application.activity;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Random;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.ProgressDialog;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.Drawable;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.IBinder;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.View;
import android.view.Window;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.maps.GeoPoint;
import com.google.android.maps.MapActivity;
import com.google.android.maps.MapController;
import com.google.android.maps.MapView;
import com.google.android.maps.Overlay;
import com.nostradabus.R;
import com.nostradabus.application.geolocation.Configuration;
import com.nostradabus.application.geolocation.HttpUtil;
import com.nostradabus.application.geolocation.LocationTracker;
import com.nostradabus.application.geolocation.UpdatableDisplay;
import com.nostradabus.application.model.Bus;
import com.nostradabus.application.overlay.CustomItemizedOverlay;
import com.nostradabus.application.overlay.CustomOverlayItem;
import com.nostradabus.service.ApiService;
import com.nostradabus.service.CurrentLocation;

public class MainActivity extends MapActivity implements UpdatableDisplay {
    private static final String INSTANCE_STATE_SAVED = "INSTANCE_STATE_SAVED";
	private Spinner spinner;
	private ProgressDialog progressDialog;
	ProgressDialog loadingBusTimeDialog;
	private MapController mapController;
	private List<Overlay> mapOverlays;
	private Drawable drawable;
	private CustomItemizedOverlay<CustomOverlayItem> itemizedOverlay;
    private ServiceConnection locationTrackerConnection;
    private LocationTracker locationTracker;
    private boolean instanceStateSaved;
	private LocationManager locationManager;
    
	private static final String bundleMappingName = "bundleMapping";
	private MapView mapView;
    private final static String TAG = "Nostradabus:MainActivity";

	@Override
	protected void onSaveInstanceState(Bundle outState) {
		if (itemizedOverlay.getFocus() != null) {
			outState.putInt(bundleMappingName, itemizedOverlay.getLastFocusedIndex());
		}
		super.onSaveInstanceState(outState);
	}

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		this.requestWindowFeature(Window.FEATURE_NO_TITLE);
		
        Log.d(TAG, "onCreate was called.");
        if(savedInstanceState != null && savedInstanceState.getBoolean(INSTANCE_STATE_SAVED)) {
            this.instanceStateSaved = true;
            Log.d(TAG, "Saved instance state is available - we were probably restarted (e.g. display orientation changed or keypad (de-)activated).");
        }
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
	    initMap();
	    initLocationTracker();

		Button startLocating = (Button) findViewById(R.id.button_start);
		startLocating.setOnClickListener(startLocatingClickListener);
		
		Button stopLocating = (Button) findViewById(R.id.button_stop);
		stopLocating.setOnClickListener(stopLocatingClickListener);
		
		if (savedInstanceState == null) {
			centerAllPoints();
		} else {
			int focused = savedInstanceState.getInt(bundleMappingName, -1);
			if (focused >= 0) {
				itemizedOverlay.setFocus(itemizedOverlay.getItem(focused));
			}
		}

		spinner = (Spinner) findViewById(R.id.bus_list);

		loadingBusTimeDialog = new ProgressDialog(MainActivity.this);
		loadingBusTimeDialog.setMessage(getResources().getString(R.string.lbl_loading_bus_time));
		loadingBusTimeDialog.setIndeterminate(true);
		loadingBusTimeDialog.setCancelable(false);

		progressDialog = new ProgressDialog(MainActivity.this);
		progressDialog.setMessage(getResources().getString(R.string.lbl_loading_bus_list));
		progressDialog.setIndeterminate(true);
		progressDialog.setCancelable(false);
		progressDialog.show();

		locationManager = (LocationManager) this.getSystemService(Context.LOCATION_SERVICE);
		locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 0, 0, locationListener);
	}

	// Define a listener that responds to location updates
	LocationListener locationListener = new LocationListener() {
	    public void onLocationChanged(Location location) {
			locationManager.removeUpdates(this);

			new BusesAsyncTask(location).execute();
	    }

		public void onStatusChanged(String provider, int status, Bundle extras) {}

	    public void onProviderEnabled(String provider) {}

	    public void onProviderDisabled(String provider) {}
	};

	private void initLocationTracker() {
        // establish a service connection regardless of start or restart
        this.locationTrackerConnection = new ServiceConnection() {
            public void onServiceConnected(ComponentName className, IBinder service) {
                Log.d(TAG, "Connection to location tracker service established.");

                // acquire the location tracker
                locationTracker = ((LocationTracker.LocationTrackerBinder) service).getService();

                // let the location tracker know where to send display updates
                locationTracker.setUpdatableDisplay(getUpdatableDisplay());

                String gpsProvider = locationTracker.getGpsProvider();
                if(gpsProvider == null || gpsProvider.length() == 0 || ! "gps".equals(gpsProvider)) {
                	Toast.makeText(MainActivity.this, "GPS NO DISPONIBLE", Toast.LENGTH_LONG).show();
                }
                else {
                    // make sure the location tracker is configured
                    if (getConfiguration() == null) {
                        if(! isInstanceStateSaved()) {
                            Log.d(TAG, "Location tracker service is not yet configured.");
                            new DownloadConfigurationTask().execute(Configuration.getServerBaseUrl() + "/ConfigurationProvider");
                        }
                        else {
                            Log.d(TAG, "Location tracker service is not yet configured but no need to try again after restart.");
                        }
    				} else {
    					Log.d(TAG, "Location tracker service is already configured.");
    					updateTrackingID();
    					updateLocationsSentCount(locationTracker.getLocationsSent());
    					updateLastLocationSentTime(locationTracker.getLastTimePosted());
    					updateButtons();
    				}
                }
            }

			public void onServiceDisconnected(ComponentName className) {
                locationTracker = null;
            }
        };

        // explicit start of service in order to let it survive an unbind
        startService(new Intent(MainActivity.this, LocationTracker.class));

        // bind service after it has been started
        bindService(new Intent(MainActivity.this, LocationTracker.class), this.locationTrackerConnection, 0);
	}

    protected void stopLocationTracker() {
        // release the service connection regardless of stop or resume since it will have to be reestablished after restart anyway
        Log.d(TAG, "Terminate service connection.");
        unbindService(this.locationTrackerConnection);
        
        if (this.instanceStateSaved) {
            Log.d(TAG, "We are going to be suspended temporarily (e.g. display orientation changed or keypad (de-)activated).");
		} else {
			if (locationTracker != null && locationTracker.isRunning()) {
				Log.d(TAG, "Location tracker service is running - leave it running and send a notification as reminder.");
				//notifyUser(R.string.notification_service_running_in_background, getText(R.string.notification_service_running_in_background));
			} else {
				Log.d(TAG, "The location tracker service is used but not running - it's safe to stop it.");
				stopService(new Intent(MainActivity.this, LocationTracker.class));
			}
		}
        locationTracker = null;
    }
    
    private UpdatableDisplay getUpdatableDisplay() {
		return this;
	}

    private void setConfiguration(Configuration configuration) {
        if (locationTracker != null) {
            locationTracker.setConfiguration(configuration);
            
            // make sure that changed preferences take effect
            SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(this);
            sharedPreferences.registerOnSharedPreferenceChangeListener(configuration);
            
            // set initial values from preferences
            configuration.setTimeInterval(sharedPreferences);
            configuration.setDistance(sharedPreferences);
        }
        if (configuration.getMessageToUsers() != null && configuration.getMessageToUsers().length() > 0) {
            //notifyUser(R.string.notification_message_to_users, configuration.getMessageToUsers());
        }
    }

    private Configuration getConfiguration() {
        if (locationTracker != null) {
            return locationTracker.getConfiguration();
        } else {
            return null;
        }
    }

    protected boolean isInstanceStateSaved() {
        return instanceStateSaved;
    }

	private void initMap() {
		mapView = (MapView) findViewById(R.id.mapview);
	    mapView.setBuiltInZoomControls(true);
	    mapView.displayZoomControls(true);

	    mapController = mapView.getController();
		mapOverlays = mapView.getOverlays();
		
		drawable = getResources().getDrawable(R.drawable.marker);
		itemizedOverlay = new CustomItemizedOverlay<CustomOverlayItem>(drawable, mapView);
	}

	View.OnClickListener startLocatingClickListener = new View.OnClickListener() {
		@Override
		public void onClick(View v) {
            if (locationTracker != null) {
                locationTracker.start();
            }
            updateButtons();
		}
	};

	View.OnClickListener stopLocatingClickListener = new View.OnClickListener() {
		@Override
		public void onClick(View v) {
            if (locationTracker != null) {
                locationTracker.stop();
            }
            updateButtons();
		}
	};

	private void centerAllPoints() {
		
		Location myLocation = CurrentLocation.getInstance().getLocation();
		if(myLocation != null) {
			GeoPoint point = new GeoPoint((int)(myLocation.getLatitude()*1e6),(int)(myLocation.getLongitude()*1e6));
			
			mapController.animateTo(point);
			mapController.setZoom(16);
		}
	}

	@Override
	protected void onDestroy() {
		super.onDestroy();
		if(progressDialog != null) {
			progressDialog.dismiss();
			progressDialog = null;
		}
	}
	
	@Override
	protected void onStart() {
		super.onStart();
	}

	private class BusesAsyncTask extends AsyncTask<Void, Void, List<Bus>> {
		private Location currentLocation;
		
		public BusesAsyncTask(Location currentLocation) {
			this.currentLocation = currentLocation;
		}
		
		@Override
		protected List<Bus> doInBackground(Void... arg0) {
			List<Bus> buses = null;
			try {
				buses = ApiService.getNearestBuses(currentLocation.getLatitude(), currentLocation.getLongitude());
			} catch (Exception e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			return buses;
		}
		
		@Override
		protected void onPostExecute(List<Bus> buses) {
			super.onPostExecute(buses);

			if(buses != null) {
				Bus emptyBus = new Bus();
				emptyBus.setId(0L);
				emptyBus.setNumero(0);
				buses.add(emptyBus);
				Collections.sort(buses);
	
				List<String> busNumberList = new ArrayList<String>(); 
				for(Bus bus : buses) {
					if(bus.getNumero() > 0) {
						busNumberList.add(bus.getNumero()+"");
					} else {
						busNumberList.add("Elegir...");
					}
				}
	
				// Create an ArrayAdapter using the string array and a default spinner layout
				ArrayAdapter<String> adapter = new ArrayAdapter<String>(
						MainActivity.this, android.R.layout.simple_spinner_dropdown_item, busNumberList);
				// Specify the layout to use when the list of choices appears
				adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
				// Apply the adapter to the spinner
				spinner.setAdapter(adapter);
				
				spinner.setOnItemSelectedListener(new OnItemSelectedListener() {
				    @Override
				    public void onItemSelected(AdapterView<?> parentView, View selectedItemView, int position, long id) {
						((TextView) parentView.getChildAt(0)).setTextColor(Color.LTGRAY);
						
				    	if(position > 0) {
				    		new ShowRemainingTimeAsyncTask().execute();
				    	}
				    }

					@Override
				    public void onNothingSelected(AdapterView<?> parentView) {
				        // your code here
				    }

				});
			}
			
			if(progressDialog != null) {
	        	progressDialog.dismiss();
	        	progressDialog = null;
			}
		}

	}
	
    private class ShowRemainingTimeAsyncTask extends AsyncTask<String, Void, Void> {

    	@Override
    	protected void onPreExecute() {
    		super.onPreExecute();
	    	loadingBusTimeDialog.show();
    	}
    	
		@Override
		protected Void doInBackground(String... params) {
			try {
				Thread.sleep(2000);
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			return null;
		}

		@Override
		protected void onPostExecute(Void result) {
			super.onPostExecute(result);
			((TextView)findViewById(R.id.bus_time_left_title)).setVisibility(View.VISIBLE);
			((Button)findViewById(R.id.me_subi_button)).setVisibility(View.VISIBLE);
			
	    	Typeface tf = Typeface.createFromAsset(getAssets(), "fonts/DS-DIGI.TTF");
	    	TextView tv = (TextView)findViewById(R.id.bus_time_left);
	        tv.setTypeface(tf);
	    	tv.setText(randInt(1,5)+"' "+randInt(10,59)+"\"");

			loadingBusTimeDialog.dismiss();
		}

    }

	public static int randInt(int min, int max) {
	    Random rand = new Random();
	    return rand.nextInt((max - min) + 1) + min;
	}

	public void removeItemOverlay() {
		itemizedOverlay.clear();
		mapOverlays.clear(); 
	}

	@Override
	protected boolean isRouteDisplayed() {
		// TODO Auto-generated method stub
		return false;
	}

    private class DownloadConfigurationTask extends AsyncTask<String, Void, Configuration> {

        private boolean configurationReceived = false;

        protected Configuration doInBackground(String... params) {
            Log.d(TAG, "Downloading configuration ...");
            Configuration configuration = null;
            try {
                String configurationString = HttpUtil.get(params[0]);
                Log.d(TAG, "... configuration downloaded.");
                if (configurationString != null) {
                    configuration = new Configuration(configurationString);
                    configurationReceived = true;
                }
            } catch (Exception e) {
                Log.e(TAG, e.getMessage());
            }
            return configuration;
        }

        protected void onPostExecute(Configuration result) {
            if (!configurationReceived) {
                Toast.makeText(MainActivity.this, "Server not available", Toast.LENGTH_LONG)
                        .show();
            } else {
                if (result.isMatchingServerApiVersion()) {
                    setConfiguration(result);
                    updateTrackingID();
                    updateButtons();
                } else {
                    Toast.makeText(MainActivity.this, "Version code mismatch", Toast.LENGTH_LONG)
                            .show();
                }
            }
        }
    }

    private void setStartButtonEnabled(boolean enabled) {
        Button button = (Button) findViewById(R.id.button_start);
        button.setEnabled(enabled);
        Log.d(TAG, "setStartButtonEnabled: " + enabled);
    }

    private void setStopButtonEnabled(boolean enabled) {
        Button button = (Button) findViewById(R.id.button_stop);
        button.setEnabled(enabled);
        Log.d(TAG, "setStopButtonEnabled: " + enabled);
    }

    public void updateTrackingID() {
//        TextView idField = (TextView) findViewById(R.id.field_trackingID);
//        idField.setText(getConfiguration().getID());
//
//        TextView url = (TextView) findViewById(R.id.label_website_url);
//        url.setText(Configuration.getServerBaseUrl() + "/?trackingID=" + getConfiguration().getID());
    }

    public void updateLocationsSentCount(Integer count) {
//        String value = getString(R.string.field_locationsSentCount);
//        if(count != null) {
//            value = "" + count;
//        }
//        TextView field = (TextView) findViewById(R.id.field_locationsSentCount);
//        field.setText(value);
    }

    public void updateLastLocationSentTime(Long timeInMillis) {
//        String value = getString(R.string.field_lastLocationSentTime);
//        if (timeInMillis != null) {
//            String timeFormat = null;
//            if (DateFormat.is24HourFormat(this)) {
//                timeFormat = "HH:mm:ss";
//            } else {
//                timeFormat = "hh:mm:ss a";
//            }
//
//            Date date = new Date(timeInMillis);
//            SimpleDateFormat formatter = new SimpleDateFormat(timeFormat);
//            value = formatter.format(date); 
//        }
//        TextView field = (TextView) findViewById(R.id.field_lastLocationSentTime);
//        field.setText(value);
    }

    public void updateTrackerCount(Integer count) {
//        String value = getString(R.string.field_trackerCount);
//        if(count != null) {
//            value = "" + count; 
//        }
//        TextView field = (TextView) findViewById(R.id.field_trackerCount);
//        field.setText(value);
    }

    private void updateButtons() {
        if (locationTracker.isRunning()) {
            setStartButtonEnabled(false);
            setStopButtonEnabled(true);
        } else {
            setStartButtonEnabled(true);
            setStopButtonEnabled(false);
        }
    }

	@Override
	public void updateMapPoint(Double latitude, Double longitude) {
		// TODO Auto-generated method stub
		
	}

    // ~ Notifications --------------------------------------------------------

    protected void notifyUser(int id, CharSequence text) {
        PendingIntent contentIntent = PendingIntent.getActivity(this, 0, new Intent(this, MainActivity.class), 0);

        Notification notification = new Notification(R.drawable.icon, text, System.currentTimeMillis());
        notification.setLatestEventInfo(this, getText(R.string.app_name), text, contentIntent);

        NotificationManager notificationManager = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
        notificationManager.notify(id, notification);
    }

}
