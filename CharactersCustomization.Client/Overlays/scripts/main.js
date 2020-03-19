 //nfive = { on: function (a, b) { } }

const CameraPositionEnum = {
	FACE: 0,
	TORSO: 1,
	LEGS: 2,
	SHOES: 3,
	FULLBODY: 4
}

nfive.on("LOG", logStr => {
	console.log(logStr)
})

function setCameraPosition(cameraPosition) {
	nfive.send("setCameraPosition", cameraPosition)
}

function onMenuOpen(menu) {
	var cameraPosition
	switch (menu) {
		case "#face":
		case "#features":
		case "#hair": {
			cameraPosition = CameraPositionEnum.FACE
			break
		}

		case "#clothes":
		case "#stats": {
			cameraPosition = CameraPositionEnum.FULLBODY
			break
		}

		default: {
			cameraPosition = CameraPositionEnum.FULLBODY
			break
		}
	}

	setCameraPosition(cameraPosition)
}

nfive.on("ready", (character) => {
	console.log("nfive is ready", character)
	nfive.show()
	/*
	$("#mother-heritage li[data-value='"+heritage.Parent1+"']").click()
	$("#father-heritage li[data-value='"+heritage.Parent2+"']").click()
	$("#resemblance-range").val(heritage.Resemblance)
	$("#skin-tone-range").val(heritage.SkinTone)
	*/
})

nfive.on("sync", function(character) {
	// TODO update all inputs with the character's values
})

$("#saveCharacter").click(function () {
	nfive.send("saveCharacter")
})

// Scroll horizontally without holding shift
$(".image-list").mousewheel(function(e, delta) {
	this.scrollLeft -= (delta * 80);
	e.preventDefault();
})

// Listener to show a menu based on a bubble click
$(".menu-bubbles .bubble").click(function() {
	var clickedBubble = $(this)

	$(".menu-bubbles .bubble.is-active").each(function() {
		$(this).removeClass("is-active")
	})
	clickedBubble.addClass("is-active")

	var sideMenuTitle = clickedBubble.data("menu-title")
	var menu = clickedBubble.data("target")
	$(".side-menu").find(".menu").each(function() {
		$(this).hide()
	})
	$("#side-menu-title").text(sideMenuTitle)
	$(menu).show()
	onMenuOpen(menu)

})

// General listener for clickable image lists to set active image
$(".image-list.clickable-items li").click(function () {
	var clickedImage = $(this)
	clickedImage.closest(".image-list").find(".is-active").each(function () {
		$(this).removeClass("is-active")
		$(this).find(".image-check").remove()
	})
	clickedImage.addClass("is-active")
	var checkClone = $("#image-check-template").clone()
	checkClone.attr("id", undefined)
	checkClone.show()
	checkClone.appendTo(clickedImage)
})

// Each color in a color palette is set dynamically based on its data attribute
$(".color-palette .color").each(function() {
	var $this = $(this)
	$this.css("color", $this.data("color"))
})

// Set mother heritage
$("#mother-heritage li").click(function() {
	var motherHeritage = parseInt($(this).data("value"))
	console.log("setMotherHeritage: " + motherHeritage)
	nfive.send("setMotherHeritage", motherHeritage)
})
// Set father heritage
$("#father-heritage li").click(function() {
	var fatherHeritage = parseInt($(this).data("value"))
	console.log("setFatherHeritage: " + fatherHeritage)
	nfive.send("setFatherHeritage", fatherHeritage)
})
// Set heritage resemblance
$(document).on("input", "#resemblance-range", function() {
	var resemblance = parseFloat($(this).val())
	console.log("setHeritageResemblance: " + resemblance)
	nfive.send("setHeritageResemblance", resemblance)
})
// Set heritage skin tone
$(document).on("input", "#skin-tone-range", function() {
	var skinTone = parseFloat($(this).val())
	console.log("setHeritageSkinTone: " + skinTone)
	nfive.send("setHeritageSkinTone", skinTone)	
})

// Set feature color, setPedHeadOverlayColor
$(".color-palette[data-target='feature-color'] .color").click(function() {
	var $this = $(this)
	var colorPalette = $this.closest(".color-palette")
	var overlayId = parseInt(colorPalette.data("overlay-id"))
	var colorType = parseInt(colorPalette.data("color-type"))
	var colorId = parseInt($this.data("value"))

	var data = { overlayId: overlayId, colorType: colorType, colorId: colorId }
	console.log("setPedHeadOverlayColor: ", data)
	nfive.send("setPedHeadOverlayColor", data)
})

// setPedHeadOverlayIndex
$(document).on("input", "input[type='range'][data-target='setPedHeadOverlayIndex']", function() {
	var $this = $(this)
	var target = $this.data("target")
	var overlayId = parseInt($this.data("overlay-id"))

	var data = { overlayId: overlayId, index: parseInt($this.val()) }
	console.log(target, data)
	nfive.send(target, data)
})
// setPedHeadOverlayOpacity
$(document).on("input", "input[type='range'][data-target='setPedHeadOverlayOpacity']", function() {
	var $this = $(this)
	var target = $this.data("target")
	var overlayId = parseInt($this.data("overlay-id"))

	var data = { overlayId: overlayId, opacity: parseFloat($this.val()) }
	console.log(target, data)
	nfive.send(target, data)
})

// Rotate character
$(document).keydown(function (e) {
	if (e.keyCode == 37) {
		// Left arrow
		console.log("rotateCharacter: -10")
		nfive.send("rotateCharacter", -10)
	} else if (e.keyCode == 39) {
		// Right arrow
		console.log("rotateCharacter: 10")
		nfive.send("rotateCharacter", 10)
	} else if (e.keyCode == 65) {
		// A-key
		nfive.send("moveCamera", "A")
	} else if (e.keyCode == 68) {
		// D-key
		nfive.send("moveCamera", "D")
	} else if (e.keyCode == 83) {
		// S-key
		nfive.send("moveCamera", "S")
	} else if (e.keyCode == 87) {
		// W-key
		nfive.send("moveCamera", "W")
	} else if (e.keyCode == 38) {
		// Up-key
		nfive.send("moveCamera", "UP")
	} else if (e.keyCode == 40) {
		// Down-key
		nfive.send("moveCamera", "DOWN")
	} else if (e.keyCode == 32) {
		// SPACE-key
		nfive.send("moveCamera", "SPACE")
	}
})


$(".range-decrease").click(function() {
	var range = $(this).closest("input[type='range']")
	console.log(range.val())
})
