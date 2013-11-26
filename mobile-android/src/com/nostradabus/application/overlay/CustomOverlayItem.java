package com.nostradabus.application.overlay;

import com.google.android.maps.GeoPoint;
import com.google.android.maps.OverlayItem;

public class CustomOverlayItem extends OverlayItem {

	protected String mImageURL;
	
	public CustomOverlayItem(GeoPoint point, String title, String snippet) {
		super(point, title, snippet);
	}
}
