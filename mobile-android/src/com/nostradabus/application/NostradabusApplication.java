package com.nostradabus.application;

import android.app.Application;

public class NostradabusApplication extends Application {
	
	private static NostradabusApplication _application;
	
	public NostradabusApplication() {
		super();
		_application = this;
	}
	public static NostradabusApplication get() {
		return _application;
	}
}
