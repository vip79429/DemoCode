public class GameStartActivity extends Activity {

	private void checkStone(int userStone) {
		// TODO Auto-generated method stub

		if (settings.getInt("hp_shop", 0) != 0){
			settings.edit().putInt("hp_shop", settings.getInt("hp_shop", 0) - 1).commit();
		}else{
			hp_level--;
		}
		hp_total = hp_level + settings.getInt("hp_shop", 0);
		Function_hpBar_cut();
		
		gamestartText_hp.setText(String.valueOf(hp_total));
		
		
		gamestartButton_stone1.setEnabled(false);
		gamestartButton_stone2.setEnabled(false);
		gamestartButton_stone3.setEnabled(false);
		handler.removeCallbacks(AIRunnable);

		int indentify = getResources().getIdentifier(
				"Level" + String.valueOf(LevelProcess) + "_" + LevelCharacterProcess + "_win_chances", "string",
				"com.example.mora");
		String temp = getResources().getString(indentify);
		int chance = Integer.valueOf(temp);

		if (isMyServiceRunning(DogService.class)){
			chance = chance + 10; //勝率多10%
		}
		
		indentify = getResources().getIdentifier(
				"Level" + String.valueOf(LevelProcess) + "_" + LevelCharacterProcess + "_win_count", "string",
				"com.example.mora");
		String cross = getResources().getString(indentify);
		
		int r = (int) (Math.random() * 100 + 1); // 猜拳機率
		winCount++;
		
		if (true) { //單機測試必勝
			if (userStone == 1) {
				gamestartButton_start.setBackgroundResource(R.drawable.gamestart_pc_stone2);
			} else if (userStone == 2) {
				gamestartButton_start.setBackgroundResource(R.drawable.gamestart_pc_stone3);
			} else if (userStone == 3) {
				gamestartButton_start.setBackgroundResource(R.drawable.gamestart_pc_stone1);
			}
			gamestartImageView_result.setImageBitmap(Function_BitmapScale(this, getResources()
					.getIdentifier("gamestart_win_" + ((int) (Math.random() * 3) + 1),
							"drawable", "com.example.mora")));
			
			//贏
			LevelCharacterProcessWin = true;
			handler.postDelayed(addStarRunnable, 3000);
		} else { // lose
			r = (int) (Math.random() * 2);
			if (userStone == 1) {
				if (r == 0) { // 平手
					gamestartButton_start.setBackgroundResource(R.drawable.gamestart_pc_stone1);
					gamestartImageView_result.setImageBitmap(Function_BitmapScale(this, R.drawable.gamestart_tie));
				} else {
					gamestartButton_start.setBackgroundResource(R.drawable.gamestart_pc_stone3);
					gamestartImageView_result.setImageBitmap(Function_BitmapScale(this, getResources()
							.getIdentifier("gamestart_failed_"+ ((int) (Math.random() * 2) + 1),
									"drawable", "com.example.mora")));
				}
			} else if (userStone == 2) {
				if (r == 0) {
					gamestartButton_start.setBackgroundResource(R.drawable.gamestart_pc_stone1);
					gamestartImageView_result.setImageBitmap(Function_BitmapScale(this, getResources()
							.getIdentifier("gamestart_failed_"+ ((int) (Math.random() * 2) + 1),
									"drawable", "com.example.mora")));
				} else { // 平手
					gamestartButton_start.setBackgroundResource(R.drawable.gamestart_pc_stone2);
					gamestartImageView_result.setImageBitmap(Function_BitmapScale(this, R.drawable.gamestart_tie));
				}
			} else if (userStone == 3) {
				if (r == 0) {
					gamestartButton_start
							.setBackgroundResource(R.drawable.gamestart_pc_stone2);
					gamestartImageView_result.setImageBitmap(Function_BitmapScale(this, getResources()
							.getIdentifier("gamestart_failed_"+ ((int) (Math.random() * 2) + 1),
									"drawable", "com.example.mora")));
				} else { // 平手
					gamestartButton_start.setBackgroundResource(R.drawable.gamestart_pc_stone3);
					gamestartImageView_result.setImageBitmap(Function_BitmapScale(this, R.drawable.gamestart_tie));
				}
			}
			//輸
			LevelCharacterProcessWin = false;
			handler.postDelayed(failedContinutRannable, 3000);
		}
		gamestartImageView_character.setEnabled(true);
		gamestartImageView_result.setVisibility(View.VISIBLE);
	}
	
	@SuppressLint("NewApi")
	final Runnable addStarRunnable = new Runnable() {
		public void run() {
			// TODO Auto-generated method stub
			gamestartImageView_character.setEnabled(false);
			gamestart_stoneControl.setVisibility(View.INVISIBLE);
			gamestartButton_cross.setEnabled(false);
			gamestartButton_start.setBackgroundResource(R.drawable.gamestart_bt_start);
			gamestartButton_stone1.setBackgroundResource(R.drawable.gamestart_stone11);
			gamestartButton_stone2.setBackgroundResource(R.drawable.gamestart_stone21);
			gamestartButton_stone3.setBackgroundResource(R.drawable.gamestart_stone31);
			gamestartImageView_result.setVisibility(View.INVISIBLE);
			
			gamestartImageView_star[LevelCharacterProcess - 1].setVisibility(View.VISIBLE);
			Animation amStar = new ScaleAnimation(2.5f, 1.0f, 2.5f, 1.0f, Animation.RELATIVE_TO_SELF, 0.5f,  
                    Animation.RELATIVE_TO_SELF, 0.5f);
			amStar.setDuration(900);
			amStar.setRepeatCount(0);
			amStar.setFillAfter(true);
			gamestartImageView_star[LevelCharacterProcess - 1].startAnimation(amStar);
			
			amStar.setAnimationListener(new Animation.AnimationListener() {
				
				public void onAnimationStart(Animation animation) {
					// TODO Auto-generated method stub
					
				}
				
				public void onAnimationRepeat(Animation animation) {
					// TODO Auto-generated method stub
					
				}
				
				public void onAnimationEnd(Animation animation) {
					// TODO Auto-generated method stub
					if (LevelCharacterProcess >= 6){
						settings.edit().putInt("AlbumOpen", 1).commit();
					}
					
					if (LevelCharacterProcess < 10){
						LevelCharacterProcess ++;
						LevelMSProcess = 1;
						
						gamestartImageView_camera.setVisibility(View.VISIBLE);
			
						new SceneAnimation(GameStartActivity.this, gamestartImageView_camera, new int[]{
								R.drawable.gamestart_camera1,
								R.drawable.gamestart_camera2,
								R.drawable.gamestart_camera3,
								R.drawable.gamestart_camera4,
								R.drawable.gamestart_camera5,
								R.drawable.gamestart_camera6
								}, metrics) {
							
							@Override
							void onAnimationFinish() {
								// TODO Auto-generated method stub
								Bitmap bitmap = Function_BitmapScale(GameStartActivity.this, getResources().getIdentifier(
										"level" + String.valueOf(LevelProcess) + "_"+ LevelCharacterProcess, "drawable",
										"com.example.mora"));
								gamestartImageView_character.setImageBitmap(Bitmap.createScaledBitmap(bitmap, (int) (metrics.widthPixels * 0.8), (int) (metrics.heightPixels * 0.72), true));

								new SceneAnimation(GameStartActivity.this, gamestartImageView_camera, new int[]{
										R.drawable.gamestart_camera6,
										R.drawable.gamestart_camera7,
										R.drawable.gamestart_camera8,
										R.drawable.gamestart_camera9
										}, metrics) {
									
									@Override
									void onAnimationFinish() {
										// TODO Auto-generated method stub
					                	gamestartImageView_camera.setVisibility(View.INVISIBLE);
					                	if (hp_total ==0){
					                		handler.postDelayed(new Runnable() {
					                			public void run() {
					                				gamestartButton_shop.performClick();
					                				shopImageView_ms_title.setImageBitmap(Function_BitmapScale(GameStartActivity.this, R.drawable.gamestart_ms_shop2));
					                			}
					                		}, 500);
					                	}else{
					                		handler.postDelayed(ShowMSRunnable, 800);
					                	}
									}
								};
							}
						};    
					}else{
						//第10關過關
						LevelCharacterProcess ++;
						gamestartImageView_level_result[0].setImageBitmap(Function_BitmapScale(GameStartActivity.this, R.drawable.gamestart_level_win1));
						handler.postDelayed(ShowMSRunnable, 800);
					}
					winCount = 0;
					gamestart_starControl.setVisibility(View.INVISIBLE);
				}
			});
		}
	};
