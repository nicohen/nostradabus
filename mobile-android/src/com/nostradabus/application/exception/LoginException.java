package com.nostradabus.application.exception;

public class LoginException extends Exception {

	private static final long serialVersionUID = 1L;
	
	private Integer responseCode;
	
	public LoginException(String m, Throwable t) {
		super(m,t);
	}
	
	public LoginException(String m) {
		super(m);
	}

	public LoginException(String m, Integer responseCode) {
		super(m);
		this.responseCode = responseCode;
	}

	public Integer getResponseCode() {
		return responseCode;
	}
	
}
