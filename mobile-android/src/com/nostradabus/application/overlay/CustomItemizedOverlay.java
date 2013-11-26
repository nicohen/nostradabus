package com.nostradabus.application.overlay;

import java.util.ArrayList;

import android.content.Context;
import android.content.Intent;
import android.graphics.drawable.Drawable;
import android.location.Location;
import android.net.Uri;

import com.google.android.maps.GeoPoint;
import com.google.android.maps.MapView;
import com.google.android.maps.OverlayItem;
import com.nostradabus.service.CurrentLocation;
import com.readystatesoftware.mapviewballoons.BalloonItemizedOverlay;
import com.readystatesoftware.mapviewballoons.BalloonOverlayView;

public class CustomItemizedOverlay<Item extends OverlayItem> extends BalloonItemizedOverlay<CustomOverlayItem> {

	private ArrayList<CustomOverlayItem> mOverlays = new ArrayList<CustomOverlayItem>();
	private Context mContext;
	
	public CustomItemizedOverlay(Drawable defaultMarker, MapView mapView) {
		super(boundCenter(defaultMarker), mapView);
		this.mContext = mapView.getContext();
		setShowClose(false);
		setShowDisclosure(true);
		populate();
	}

	public void addOverlay(CustomOverlayItem overlay) {
		mOverlays.add(overlay);
		setLastFocusedIndex(-1);
	    populate();
	}

	@Override
	protected CustomOverlayItem createItem(int i) {
		return mOverlays.get(i);
	}

	public void clear() {
        mOverlays.clear();
        setLastFocusedIndex(-1);
        populate();
    }

	@Override
	public int size() {
		return mOverlays.size();
	}

	@Override
	protected boolean onBalloonTap(int index, CustomOverlayItem item) {
		Location currentPoint = CurrentLocation.getInstance().getLocation();
		if(currentPoint != null) {
			GeoPoint mapPoint = item.getPoint();
	        final Intent intent = new Intent(Intent.ACTION_VIEW, 
	        		Uri.parse("http://maps.google.com/maps?saddr="+mapPoint.getLatitudeE6()/1e6+","+mapPoint.getLongitudeE6()/1e6+
	        				"&daddr="+currentPoint.getLatitude()+","+currentPoint.getLongitude()));
	        intent.setClassName("com.google.android.apps.maps", "com.google.android.maps.MapsActivity");

	        mContext.startActivity(intent);
		} else {
			this.hideBalloon();
		}
		return true;
	}

	@Override
	protected BalloonOverlayView<CustomOverlayItem> createBalloonOverlayView() {
		return new BalloonOverlayView<CustomOverlayItem>(getMapView().getContext(), getBalloonBottomOffset());
	}

}
