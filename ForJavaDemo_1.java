public class Stand_HistoryDetail extends Activity{

	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		Stand_MasterPage.Current_Page = Page_Index.basicset;
		orderStr = "";
		mContext = Stand_HistoryDetail.this;
		orderStrSets = "";
		orderStrAPDetail = "";
		mActivity = getParent();
		loadHistoryDetail();
		
	}

	private void loadHistoryDetail()
	{

		mSqlBranch = new SQL_BranchInformation(this);
		mSqlRestaurant = new SQL_Restaurant(this);
		mSqlMenuitem = new SQL_MenuItem(this);

		mSqlSets = new SQL_Sets(this);
		mSqlAPDetail = new SQL_AdditionalPropertiesDetail(this);
		mSqlSetsMenuitem = new SQL_Sets_MenuItems(this);
		mSqlTYPE_SETMenuitem = new SQL_TYPE_SET_MenuItem(this);
		mpopwindowBtn=new popwindowBtn();
		
		if (Stand_Login.logged) {
			mdialog = new ProgressDialog(Stand_MasterPage.MasterContext);
			mdialog.setTitle("取資料中");
			mdialog.setMessage("請稍候!");
			mdialog.show();

			new Thread(new Runnable() {
				public void run() {
					Json_connection mJson = new Json_connection();

					Cursor mCursor_M = null,mCuror_B, mCursro_R;
					JSONObject UserHistory = mJson
							.Get_OrderdedDetails_Test(Stand_OrderedHistory.historyKEY);
					Log.e("Stand_OrderedHistory.historyKEY",Stand_OrderedHistory.historyKEY);

					if(UserHistory==null)
					{
						Message msg = new Message();
						msg.what = 0x001;
						mHandler.sendMessage(msg);
						return;
					}
					history = new String[UserHistory.length()];
					try {

						if(UserHistory.getString("orderType").toLowerCase().equals("dine-in") || UserHistory.getString("orderType").toLowerCase().equals("take-out"))
						{
							talg=true;

							mCuror_B = mSqlBranch.getBranchById(UserHistory.getString("branchKey"));
							if(mCuror_B.getCount()>0)
								mCuror_B.moveToPosition(0);
							mCursro_R =  mSqlRestaurant.getRestaurantByID(mCuror_B.getString(SQL_BranchInformation.index.restaurantId));
							if(mCursro_R.getCount()>0)
								mCursro_R.moveToPosition(0);
							
							
							history[0] = UserHistory.getString("status");
							history[1] = UserHistory.getString("orderTime");
							history[2] = mCursro_R.getString(SQL_Restaurant.index.restaurantName);
							history[3] = mCuror_B.getString(SQL_BranchInformation.index.branchName);
							history[4] = UserHistory.getString("orderType");
							history[5] = UserHistory.getString("timeToServe");
							history[6] = UserHistory.getString("numberOfPeople");
							history[7] = UserHistory.getString("totalPrice");
			
							history[12] = UserHistory.getString("orderPaymentType");
							Log.e("history", String.valueOf(history)); 
							JSONArray jsonArray_orderDetails = UserHistory
									.getJSONArray("orderDetails");
							historyDetail = new String[jsonArray_orderDetails
									.length()][5];
							for (int branch_index = 0; branch_index < jsonArray_orderDetails
									.length(); branch_index++) {
								
								mCursor_M = mSqlMenuitem.getMenuItemById(jsonArray_orderDetails
										.getJSONObject(branch_index).getString(
												"menuItemKey"));
								if(mCursor_M.getCount()>0)
									mCursor_M.moveToPosition(0);
								
								historyDetail[branch_index][0] = mCursor_M
										.getString(SQL_MenuItem.index.menuItemName);
								historyDetail[branch_index][1] = mCursor_M
										.getString(SQL_MenuItem.index.menuItemPrice);
								historyDetail[branch_index][2] = mCursor_M
										.getString(SQL_MenuItem.index.menuItemDiscount);
								historyDetail[branch_index][3] = jsonArray_orderDetails
										.getJSONObject(branch_index).getString(
												"quantity");
								historyDetail[branch_index][4] = jsonArray_orderDetails
										.getJSONObject(branch_index).getString("menuItemKey");
							}
							if (mCursor_M!=null)
								mCursor_M.close();
							
				
							JSONArray jsonArray_orderSets = UserHistory
								.getJSONArray("orderSetDetails");
							historySets = new String[jsonArray_orderSets.length()][5];
							TYPEMenuIteml = new String[jsonArray_orderSets.length()];
							for (int i=0;i<TYPEMenuIteml.length;i++){
								TYPEMenuIteml[i] = "";
							}
							
								for (int branch_index = 0; branch_index < jsonArray_orderSets.length(); branch_index++) {
													
									mCursor_M = mSqlSets.getMenuItemById(jsonArray_orderSets
											.getJSONObject(branch_index).getString("setKey"));
									if(mCursor_M.getCount()>0)
										mCursor_M.moveToPosition(0);
									
									historySets[branch_index][0] = mCursor_M
										.getString(SQL_Sets.index.setName);
									historySets[branch_index][1] = mCursor_M
										.getString(SQL_Sets.index.setPrice);
									historySets[branch_index][2] = mCursor_M
										.getString(SQL_Sets.index.setDiscount);
									historySets[branch_index][3] = jsonArray_orderSets
										.getJSONObject(branch_index).getString("quantity");
									historySets[branch_index][4] = jsonArray_orderSets
										.getJSONObject(branch_index).getString("setKey");
									
									Cursor mCursorSMenuItem = mSqlSetsMenuitem.getMenuItemById(
											mCursor_M.getString(SQL_Sets.index.menuItemId));
									
									historySets[branch_index][0] = historySets[branch_index][0] + "\n";
									if (mCursorSMenuItem.getCount() > 0){
											
										for (int i=0;i<mCursorSMenuItem.getCount();i++){
											mCursorSMenuItem.moveToPosition(i);
											
											mCursor_M = mSqlMenuitem.getMenuItemById(
													mCursorSMenuItem.getString(SQL_Sets_MenuItems.index.SetmenuItemId));
											mCursor_M.moveToPosition(0);
											
											historySets[branch_index][0] = historySets[branch_index][0] +
												mCursor_M.getString(SQL_MenuItem.index.menuItemName) + " ";
										}
									}
										
									JSONArray jsonArray_orderSets_TYPE =jsonArray_orderSets
										.getJSONObject(branch_index).getJSONArray("typeSetMenuItemKeys");
									TPYEMenuItemlength = jsonArray_orderSets_TYPE.length();

									for (int i=0;i<jsonArray_orderSets_TYPE.length();i++){
										Cursor mCursor_TYPE_M = mSqlTYPE_SETMenuitem.getMenuItemITEM_ID(
												jsonArray_orderSets_TYPE.getString(i));
										mCursor_TYPE_M.moveToPosition(i);
										mCursor_TYPE_M.moveToFirst();
										
										mCursor_M = mSqlMenuitem.getMenuItemById(mCursor_TYPE_M.getString(
												SQL_TYPE_SET_MenuItem.index.menuItemId));
										mCursor_M.moveToPosition(i);
										mCursor_M.moveToFirst();
										
										historySets[branch_index][0] = historySets[branch_index][0] +
											mCursor_M.getString(SQL_MenuItem.index.menuItemName) + " ";
										TYPEMenuIteml[branch_index] = TYPEMenuIteml[branch_index]
										                                            + jsonArray_orderSets_TYPE.getString(i) + ",";
										Log.e(String.valueOf(branch_index), TYPEMenuIteml[branch_index]);
									}
										
									
								}

								if (mCursor_M!=null)
									mCursor_M.close();
								
							JSONArray jsonArray_orderAPDetail = UserHistory
								.getJSONArray("orderAdditionalPropertyDetails");
							APKEY = new String[jsonArray_orderAPDetail.length()];
							for (int i=0;i<APKEY.length;i++){
								APKEY[i] = "";
							}
							
							historyAPDetail = new String[jsonArray_orderAPDetail.length()][5];
								for (int branch_index = 0; branch_index < jsonArray_orderAPDetail.length(); branch_index++) {
					
									
									JSONArray jsonArray_orderAPDetailMenuItem = jsonArray_orderAPDetail
									.getJSONObject(branch_index)
									.getJSONArray("menuItemAdditionalPropertyValueKeys");
									
									mCursor_M = mSqlAPDetail.getadditionalPropertykey(
											jsonArray_orderAPDetailMenuItem.getString(0));
								
									
									mCursor_M.moveToPosition(branch_index);
									mCursor_M.moveToFirst();
						
									Cursor mCursor_APMenuItem = mSqlMenuitem.getMenuItemById(
											mCursor_M.getString(SQL_AdditionalPropertiesDetail.index.menuItemId));
						
									mCursor_APMenuItem.moveToPosition(branch_index);
									mCursor_APMenuItem.moveToFirst();
			
									historyAPDetail[branch_index][0] = 
										mCursor_APMenuItem.getString(SQL_MenuItem.index.menuItemName) + "\n";
									historyAPDetail[branch_index][1] = 
										mCursor_APMenuItem.getString(SQL_MenuItem.index.menuItemPrice);
									historyAPDetail[branch_index][2] = 
										mCursor_APMenuItem.getString(SQL_MenuItem.index.menuItemDiscount);
									historyAPDetail[branch_index][3] = jsonArray_orderAPDetail
									.getJSONObject(branch_index).getString("quantity");
									historyAPDetail[branch_index][4] = jsonArray_orderAPDetail
									.getJSONObject(branch_index).getString("key");
				
									for (int i=0;i<jsonArray_orderAPDetailMenuItem.length();i++){
						
										mCursor_M = mSqlAPDetail.getadditionalPropertykey(
												jsonArray_orderAPDetailMenuItem.getString(i));
										
										mCursor_M.moveToPosition(i);
										mCursor_M.moveToFirst();
										
										historyAPDetail[branch_index][0] = historyAPDetail[branch_index][0] +
											mCursor_M.getString(SQL_AdditionalPropertiesDetail.index.additionalPropertyTypeName) + " " +
											mCursor_M.getString(SQL_AdditionalPropertiesDetail.index.additionalPropertyValueName);
										if (i < jsonArray_orderAPDetailMenuItem.length() - 1)
											historyAPDetail[branch_index][0] = historyAPDetail[branch_index][0] + "\n";
										
										
										historyAPDetail[branch_index][1] = String.valueOf(Integer.parseInt(historyAPDetail[branch_index][1]) +
											(int)Float.parseFloat(mCursor_M.getString(SQL_AdditionalPropertiesDetail.index.additionalCharge)));
										
										APKEY[branch_index] = APKEY[branch_index] + 
											jsonArray_orderAPDetailMenuItem.getString(i) + ",";

									}
								}	

								if (mCursor_M!=null)
									mCursor_M.close();
								
							history[8]=UserHistory.getString("orderComments");
							history[9]=UserHistory.getString("orderDeliveryType");
							history[10] = UserHistory.getJSONObject("contactPhone").getString("number");
							history[11] = UserHistory.getJSONObject("deliveryAddress").getString("address");
	
							if (history[4].toLowerCase().equals("take-out")){
								history[4] = "takeout";
							}else if (history[4].toLowerCase().equals("dine-in")){
								history[4] = "takein";
							}else{
								history[4] = "delivery";
							}
							Log.e("history[4]", history[4]);
							if (history[9].toLowerCase().equals("regular")){
								history[9] = "regular";
							}else if (history[9].toLowerCase().equals("postal")){
								history[9] = "postal";
							}else if (history[9].toLowerCase().equals("ups")){
								history[9] = "ups";
							}else if (history[9].toLowerCase().equals("convenience_store")){
								history[9] = "convenience_store";
							}
							
		
							if(mCursor_M!=null)
								mCursor_M.close();
							mCuror_B.close();
							mCursro_R.close();
						
						}
						else
						{
							talg=false;
							
							mCuror_B = mSqlBranch.getBranchById(UserHistory.getString("branchKey"));
							if(mCuror_B.getCount()>0)
								mCuror_B.moveToPosition(0);
							mCursro_R =  mSqlRestaurant.getRestaurantByID(mCuror_B.getString(SQL_BranchInformation.index.restaurantId));
							if(mCursro_R.getCount()>0)
								mCursro_R.moveToPosition(0);
							
							history[0] = UserHistory.getString("status");
							history[1] = UserHistory.getString("orderTime");
							history[2] = mCursro_R.getString(SQL_Restaurant.index.restaurantName);
							history[3] = mCuror_B.getString(SQL_BranchInformation.index.branchName);
							history[4] = UserHistory.getString("orderType");
							history[8] = UserHistory.getString("orderComments");
							
							history[5] = UserHistory.getString("timeToServe");
							history[6] = UserHistory.getString("numberOfPeople");
							history[7] = UserHistory.getString("totalPrice");
							history[8]=UserHistory.getString("orderComments");
							history[9]=UserHistory.getString("orderDeliveryType");
							history[10] = UserHistory.getJSONObject("contactPhone").getString("number");
							history[11] = UserHistory.getJSONObject("deliveryAddress").getString("address");
							history[12] = UserHistory.getString("orderPaymentType");
							
							
							mCuror_B.close();
							mCursro_R.close();
						}
						Message msg = new Message();
						msg.what = 0x001;
						mHandler.sendMessage(msg);
					} catch (Exception e) {
						Message msg = new Message();
						msg.what = 0x002;
						mHandler.sendMessage(msg);
						e.printStackTrace();
					}
				}
			}).start();

		} else {
			login_err = new AlertDialog.Builder(Stand_MasterPage.MasterContext);
			login_err.setTitle("尚未登錄");
			login_err.setMessage("無法看到歷史訂單資訊");
			err_login = login_err.create();
			err_login.show();
			new Thread(new Runnable() {
				public void run() {
					try {
						Thread.sleep(2000);
						err_login.dismiss();
					} catch (Exception e) {
						e.printStackTrace();
					}
				}
			}).start();
		}

	}
	Handler mHandler = new Handler() {
		public void handleMessage(Message msg) {
			switch (msg.what) {
				case 0x001:
					mdialog.dismiss();
					loadModule();
					break;
				case 0x002:
					mdialog.dismiss();
					break;
			}
			super.handleMessage(msg);
		}
	};

	//異同步畫面處理
	Handler mHandler2 = new Handler() {
		public void handleMessage(Message msg) {
			switch (msg.what) {
				case 0x001:
				{
					mProgressdialog.dismiss();
					// 切換頁面
					((Stand_MasterPage)Stand_MasterPage.MasterContext).back();
					((Stand_OrderedHistory)Stand_OrderedHistory.context).onResume();
				}
					break;
				case 0x002:
				{
					mProgressdialog.dismiss();
					// 切換頁面
					mDialog.setTitle("訊息");
					mDialog.setMessage("訂單錯誤!");
					
					mDialog.setNeutralButton("OK", new DialogInterface.OnClickListener() {
						
						
						public void onClick(DialogInterface dialog, int which) {
							// TODO Auto-generated method stub
							dialog.dismiss();
						}
					});
					mDialog.show();
				}
					break;
			}
			super.handleMessage(msg);
		}
	};
	
}
