# Adaptive Image Layout Specification

## Overview
Implement responsive image grid layout for PostCard component with the following rules:
- **1 image**: Full width, maintain original aspect ratio
- **2 images**: Side by side, each 50% width
- **3 images**: Top image 100% width, bottom two 50% width each
- **4 images**: 2x2 grid, equal sizing
- **5+ images**: 2xN grid showing first 4 images with overlay on last image

## Component Changes

### Template Modifications
```html:src/frontend/thrume-frontend/src/components/PostCard.vue
<div class="post-images" :class="imageGridClass" v-if="hasImages">
  <div 
    v-for="(imageUrl, index) in displayedImages" 
    :key="imageUrl"
    class="image-container"
    @click="openGallery(index)"
  >
    <img :src="imageUrl" alt="Post image" class="post-image" />
    <div v-if="shouldShowOverlay(index)" class="image-overlay">
      +{{ hiddenImagesCount }}
    </div>
  </div>
</div>
```

### Script Logic Updates
```typescript:src/frontend/thrume-frontend/src/components/PostCard.vue
// Replace existing displayedImages, imageGridClass, and showOverlay functions

const displayedImages = computed(() => {
  return props.post.images.slice(0, 4);
});

const hiddenImagesCount = computed(() => {
  return Math.max(props.post.images.length - 4, 0);
});

const shouldShowOverlay = (index: number) => {
  return props.post.images.length > 4 && index === 3;
};

const imageGridClass = computed(() => {
  const count = displayedImages.value.length;
  
  if (count === 1) return 'single-image';
  if (count === 2) return 'two-images';
  if (count === 3) return 'three-images';
  return 'four-plus-images'; // For 4+ images
});
```

### CSS Implementation
```css:src/frontend/thrume-frontend/src/components/PostCard.vue
/* Remove existing .grid-* classes */
.single-image {
  display: block;
}

.two-images {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
}

.three-images {
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: auto;
  gap: 4px;
}

.three-images .image-container:first-child {
  grid-column: span 2;
}

.four-plus-images {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  grid-auto-rows: 1fr;
  gap: 4px;
}

.image-container {
  position: relative;
  overflow: hidden;
  cursor: pointer;
  aspect-ratio: 1/1; /* Square containers */
}

.post-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center;
  transition: transform 0.3s ease;
}

.image-container:hover .post-image {
  transform: scale(1.03);
}

.image-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: linear-gradient(0deg, rgba(0,0,0,0.7) 0%, rgba(0,0,0,0.3) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 2rem;
  font-weight: bold;
}
```

## Edge Case Handling
1. **Aspect Ratios**: 
   - All images will be cropped to square (1:1) using `object-fit: cover`
   - Maintains consistent grid appearance regardless of original aspect ratio
   
2. **Performance**:
   - Only first 4 images are rendered in DOM
   - Gallery component lazy loads additional images

3. **Responsive Behavior**:
   - Grid adjusts to container width
   - Minimum image size of 100px to prevent squishing

## Validation Checklist
- [ ] 1 image displays full width
- [ ] 2 images display side-by-side
- [ ] 3 images show top-full + bottom split
- [ ] 4 images show 2x2 grid
- [ ] 5+ images show 2x2 grid with overlay on last image
- [ ] Overlay shows correct "+N" count
- [ ] Clicking images opens gallery at correct index
- [ ] Hover effects work on all images