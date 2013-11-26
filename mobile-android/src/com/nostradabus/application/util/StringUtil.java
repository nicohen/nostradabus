package com.nostradabus.application.util;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

public class StringUtil {

	public static String encryptMd5(String foo) throws NoSuchAlgorithmException {
		String md5 = "";
		MessageDigest md = MessageDigest.getInstance("MD5");
		md.reset();
		for (byte b: md.digest(foo.getBytes())) {
			md5 += String.format("%02x", 0xFF&b);
		}
		return md5;
	}
}
